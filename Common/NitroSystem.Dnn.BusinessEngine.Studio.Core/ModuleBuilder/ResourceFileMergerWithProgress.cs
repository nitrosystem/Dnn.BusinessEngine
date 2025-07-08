using NitroSystem.Dnn.BusinessEngine.Core.Appearance;
using NitroSystem.Dnn.BusinessEngine.Core.Common;
using NitroSystem.Dnn.BusinessEngine.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.ModuleBuilder
{
    public class ResourceFileMergerWithProgress
    {
        private readonly ConcurrentDictionary<string, string> _fileCache = new();
        private readonly SemaphoreSlim _semaphore = new(4); // حداکثر ۴ فایل همزمان

        public async Task MergeAndSaveResourcesAsync(
            IEnumerable<BuildModuleResourceDto> resources,
            BuildModuleResourceType resourceType,
            Func<IEnumerable<string>, string> mergeStrategy,
            Func<string, string> outputPathResolver,
            Action<string, string>? progressCallback = null // entryType, status
        )
        {
            var grouped = resources
                .Where(r => r.ResourceType == resourceType)
                .GroupBy(r => r.EntryType);

            var tasks = grouped.Select(async group =>
            {
                var entryType = group.Key;
                var outputPath = outputPathResolver(entryType);

                await _semaphore.WaitAsync();

                try
                {
                    progressCallback?.Invoke(entryType, "Loading");

                    var contents = await Task.WhenAll(group.Select(async r =>
                    {
                        return await LoadCachedResourceAsync(r.ResourcePath);
                    }));

                    var mergedContent = mergeStrategy(contents);

                    var directory = Path.GetDirectoryName(outputPath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    await File.WriteAllTextAsync(outputPath, mergedContent);

                    progressCallback?.Invoke(entryType, "Saved");
                }
                catch (Exception ex)
                {
                    progressCallback?.Invoke(entryType, $"❌ Error: {ex.Message}");
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        private async Task<string> LoadCachedResourceAsync(string path)
        {
            if (_fileCache.TryGetValue(path, out var cachedContent))
                return cachedContent;

            var content = await File.ReadAllTextAsync(path);
            _fileCache.TryAdd(path, content);
            return content;
        }
    }

}
