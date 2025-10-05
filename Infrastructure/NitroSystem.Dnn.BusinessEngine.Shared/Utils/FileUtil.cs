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
        // کش داخلی (Thread-Safe)
        private static readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// لود کردن فایل‌ها (با کش داخلی)
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
                                    assign?.Invoke(filePath, cachedContent); // 🔥 فراخوانی assign
                                    continue;
                                }

                                if (File.Exists(resolvedPath))
                                {
                                    string content = await GetFileContentAsync(resolvedPath);
                                    _cache[resolvedPath] = content;
                                    results[filePath] = content;
                                    assign?.Invoke(filePath, content); // 🔥
                                }
                                else
                                {
                                    results[filePath] = null;
                                    assign?.Invoke(filePath, null); // 🔥
                                }
                            }
                        }
                    }, cancellationToken))
            );

            return results;
        }

        /// <summary>
        /// پاک کردن کل کش
        /// </summary>
        public static void ClearCache() => _cache.Clear();

        /// <summary>
        /// پاک کردن کش مربوط به یک فایل خاص
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
    }
}