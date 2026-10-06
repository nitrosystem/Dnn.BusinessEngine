using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Utils
{
    public static class FileUtil
    {
        // internal cache (Thread-Safe)
        private static readonly ConcurrentDictionary<string, string> _cache = new();

        /// <summary>
        /// Load files(with cache)
        /// </summary>
        public static async Task<IDictionary<string, string>> LoadFilesWithCachingAsync(
            IEnumerable<string> filePaths,
            Func<string, string> mapPath = null,
            Action<string, string> assign = null,
            int maxDegreeOfParallelism = 4,
            CancellationToken cancellationToken = default)
        {
            var results = new ConcurrentDictionary<string, string>();

            await Task.WhenAll(
                Partitioner.Create(filePaths).GetPartitions(maxDegreeOfParallelism)
                    .Select(partition => Task.Run(async () =>
                    {
                        using (partition)
                        {
                            while (partition.MoveNext())
                            {
                                var filePath = partition.Current;
                                cancellationToken.ThrowIfCancellationRequested();

                                string resolvedPath = mapPath != null
                                    ? mapPath(filePath)
                                    : filePath;

                                if (_cache.TryGetValue(resolvedPath, out var cachedContent))
                                {
                                    results[filePath] = cachedContent;
                                    assign?.Invoke(filePath, cachedContent);
                                    continue;
                                }

                                if (File.Exists(resolvedPath))
                                {
                                    string content = await GetFileContentAsync(resolvedPath);
                                    _cache[resolvedPath] = content;
                                    results[filePath] = content;
                                    assign?.Invoke(filePath, content);
                                }
                                else
                                {
                                    results[filePath] = null;
                                    assign?.Invoke(filePath, null);
                                }
                            }
                        }
                    }, cancellationToken))
            );

            return results;
        }

        public static async Task<IDictionary<string, string>> LoadFilesAsync(
          IEnumerable<string> filePaths,
          Func<string, string> mapPath = null,
          Action<string, string> assign = null,
          bool throwExecption = false)
        {
            var results = new ConcurrentDictionary<string, string>();

            foreach (var filePath in filePaths)
            {
                string resolvedPath = mapPath != null
                    ? mapPath(filePath)
                    : filePath;

                if (File.Exists(resolvedPath))
                {
                    string content = await GetFileContentAsync(resolvedPath);
                    results[filePath] = content;

                    assign?.Invoke(filePath, content);
                }
                else if (throwExecption)
                {
                    throw new FileNotFoundException($"File not found: {resolvedPath}", resolvedPath);
                }
            }

            return results;
        }

        /// <summary>
        /// Clear cache
        /// </summary>
        public static void ClearCache() => _cache.Clear();

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        public static void Invalidate(string filePath, Func<string, string> mapPath = null)
        {
            string resolvedPath = mapPath != null ? mapPath(filePath) : filePath;
            _cache.TryRemove(resolvedPath, out _);
        }

        /// <summary>
        /// Load multiple files asynchronously with limited concurrency.
        /// The caller can provide a mapPath function (e.g. HttpContext.Current.Server.MapPath)
        /// or pass null if the paths are already physical file paths.
        /// Returns a dictionary mapping the original path string to the file content (empty string if missing).
        /// </summary>
        public static async Task<IDictionary<string, string>> LoadFilesAsync(
            IEnumerable<string> relativeOrVirtualPaths,
            Func<string, string> mapPath = null,
            int maxDegreeOfParallelism = 4,
            CancellationToken cancellationToken = default)
        {
            if (relativeOrVirtualPaths == null) throw new ArgumentNullException(nameof(relativeOrVirtualPaths));
            if (maxDegreeOfParallelism <= 0) throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));

            var result = new ConcurrentDictionary<string, string>();
            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = new List<Task>();

            // deduplicate while preserving a single representative for each key
            var distinctPaths = relativeOrVirtualPaths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var path in distinctPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        var physicalPath = mapPath != null ? mapPath(path) : path;
                        string content = string.Empty;

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(physicalPath) && File.Exists(physicalPath))
                            {
                                // Use StreamReader async read (works on .NET Framework 4.8)
                                using (var fs = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                                using (var sr = new StreamReader(fs, Encoding.UTF8))
                                {
                                    content = await sr.ReadToEndAsync().ConfigureAwait(false);
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch
                        {
                            // swallow file-specific exceptions and return empty content (consistent with original behavior)
                            content = string.Empty;
                        }

                        result[path] = content;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return new Dictionary<string, string>(result); // return a snapshot (IDictionary)
        }

        /// <summary>
        /// Convenience overload for classic ASP.NET apps that want to pass HttpContext.Current.Server.MapPath automatically.
        /// If HttpContext.Current is null, behavior is the same as calling the core method with mapPath == null.
        /// </summary>
        public static Task<IDictionary<string, string>> LoadFilesAsync(
            IEnumerable<string> relativeOrVirtualPaths,
            int maxDegreeOfParallelism = 4,
            CancellationToken cancellationToken = default)
        {
            Func<string, string> mapPath = null;

            try
            {
                var ctx = System.Web.HttpContext.Current;
                if (ctx != null)
                    mapPath = ctx.Server.MapPath;
            }
            catch
            {
                // ignore and fallback to no mapping
                mapPath = null;
            }

            return LoadFilesAsync(relativeOrVirtualPaths, mapPath, maxDegreeOfParallelism, cancellationToken);
        }

        public static string GetFileContent(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                return File.ReadAllText(filePath);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<string> GetFileContentAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task WriteFileContentAsync(string filePath, string content, bool isCreateDirectory = true, bool isDeleteOldFile = true)
        {
            if (isDeleteOldFile && File.Exists(filePath))
                File.Delete(filePath);

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var writer = new StreamWriter(filePath, append: false))
            {
                await writer.WriteAsync(content);
            }
        }

        public static (bool isDeleted, Exception error) DeleteDirectory(string path, bool recursive = true)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (false, new ArgumentException("Path cannot be null or empty."));

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public static bool CreateTextFile(string fileName, string content, bool deleteIsExists)
        {
            try
            {
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(fileName) && deleteIsExists) File.Delete(fileName);

                // Check if directory is not exists. create it.     
                if (!Directory.Exists(Path.GetDirectoryName(fileName))) Directory.CreateDirectory(Path.GetDirectoryName(fileName));

                // Create a new file     
                using (FileStream fs = File.Create(fileName))
                {
                    // Add some text to file    
                    Byte[] data = new UTF8Encoding(true).GetBytes(content);
                    fs.Write(data, 0, data.Length);

                    fs.Close();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void RenameFolder(string sourcePath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));

            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentNullException(nameof(destinationPath));


            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException(
                    $"Source folder does not exist: {sourcePath}");


            if (Directory.Exists(destinationPath))
                throw new IOException(
                    $"Destination folder already exists: {destinationPath}");


            Directory.Move(sourcePath, destinationPath);
        }

        public static void MoveLeafFolders(string currentDir, string sourceRoot, string destinationRoot)
        {
            var subDirs = Directory.GetDirectories(currentDir);

            if (subDirs.Length == 0)
            {
                // Leaf folder -> replace it as a whole unit
                string relativePath = GetRelativePath(sourceRoot, currentDir);
                string destPath = Path.Combine(destinationRoot, relativePath);

                if (Directory.Exists(destPath))
                    Directory.Delete(destPath, true);

                string parent = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                    Directory.CreateDirectory(parent);

                Directory.Move(currentDir, destPath);
                return;
            }

            // Not a leaf: make sure the mirrored destination folder exists,
            // move any loose files that live directly in this folder, then recurse.
            string currentRelative = GetRelativePath(sourceRoot, currentDir);
            string currentDest = Path.Combine(destinationRoot, currentRelative);

            if (!Directory.Exists(currentDest))
                Directory.CreateDirectory(currentDest);

            foreach (var file in Directory.GetFiles(currentDir))
            {
                string destFile = Path.Combine(currentDest, Path.GetFileName(file));
                if (File.Exists(destFile))
                    File.Delete(destFile);

                File.Move(file, destFile);
            }

            foreach (var dir in subDirs)
            {
                MoveLeafFolders(dir, sourceRoot, destinationRoot);
            }
        }

        private static string GetRelativePath(string root, string fullPath)
        {
            return fullPath.Substring(root.Length)
                            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}