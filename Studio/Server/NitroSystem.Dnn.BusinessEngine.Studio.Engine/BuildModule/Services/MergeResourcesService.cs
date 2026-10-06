using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class MergeResourcesService : IMergeResourcesService
    {
        private ModuleDto _module;
        private IEngineContext _context;
        private Action<string, double> _onProgress;

        public async Task<(string Scripts, string Styles)> MergeResourcesAsync(
            IEngineContext context,
            ModuleDto module,
            IEnumerable<ModuleResourceDto> resources,
            Action<string, double> progress = null)
        {
            _module = module;
            _context = context;
            _onProgress = progress;

            var resourcesLookup = resources.ToLookup(r => r.ResourceContentType);
            var scripts = await BuildScripts(resourcesLookup[ResourceContentType.Js]);
            var styles = await BuildStyles(resourcesLookup[ResourceContentType.Css]);
            return (scripts, styles);
        }

        private async Task<string> BuildStyles(IEnumerable<ModuleResourceDto> resources)
        {
            var path = _context.Get<string>("OutputRelativePath");
            var list = new List<ModuleResourceDto>
            {
                new ModuleResourceDto()
                {
                    ModuleId = _module.Id,
                    ResourceType = ModuleResourceType.ModuleLayoutCss,
                    ResourceContentType = ResourceContentType.Css,
                    ResourcePath = $@"{path}/_layout.css",
                    EntryType = _module.ModuleName + "-ModuleLayoutCss",
                    LoadOrder = 0,
                }
            };
            list.AddRange(resources);

            var styles = new StringBuilder();
            var mergedStyles = await MergeStyleResources(list);
            styles.AppendLine(mergedStyles);
            return styles.ToString();
        }

        private async Task<string> MergeStyleResources(IEnumerable<ModuleResourceDto> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            var stylesChunks = new ConcurrentBag<string>();

            await ParallelBatchExecutor.ExecuteInParallelBatchesAsync(
                resources.Where(r => !string.IsNullOrEmpty(r.ResourcePath)),
                batchSize: 5,
                maxDegreeOfParallelism: 3,
                async batch =>
                {
                    // Each batch has its own StringBuilder.
                    var localBuilder = new StringBuilder();
                    var items = batch
                        .GroupBy(r => r.ResourcePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesAsync(
                        items.Keys,
                        GlobalHelper.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var resource))
                            {
                                // append just on localBuilder (thread-safe)
                                localBuilder.AppendLine($@"/* ----- Start Styles For {resource.EntryType} ---- */");
                                localBuilder.AppendLine(fileContent ?? string.Empty);
                                localBuilder.AppendLine($"/* ----- End Styles For {resource.EntryType} ----*/");
                                localBuilder.AppendLine();
                            }
                        });

                    //add batch result to the thread-safe collection.
                    if (localBuilder.Length > 0)
                        stylesChunks.Add(localBuilder.ToString());
                });

            // finaly, the content of all batches is combined by order.
            var finalBuilder = new StringBuilder();
            foreach (var chunk in stylesChunks)
                finalBuilder.AppendLine(chunk);

            _onProgress.Invoke($"Merged style resourcess for  {_module.ModuleName} module", 87.5);

            return finalBuilder.ToString();
        }

        private async Task<string> BuildScripts(IEnumerable<ModuleResourceDto> resources)
        {
            var scripts = new StringBuilder();
            var mergedStyles = await MergeScriptResources(resources);
            scripts.AppendLine(mergedStyles);
            return scripts.ToString();
        }

        private async Task<string> MergeScriptResources(IEnumerable<ModuleResourceDto> resources)
        {
            if (resources == null)
                throw new ArgumentNullException(nameof(resources));

            var scriptChunks = new ConcurrentBag<string>();

            await ParallelBatchExecutor.ExecuteInParallelBatchesAsync(
                resources,
                batchSize: 5,
                maxDegreeOfParallelism: 3,
                async batch =>
                {
                    // Each batch has its own StringBuilder.
                    var localBuilder = new StringBuilder();
                    var items = batch
                        .GroupBy(r => r.ResourcePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesAsync(
                        items.Keys,
                        GlobalHelper.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var resource))
                            {
                                // append just on localBuilder (thread-safe)
                                localBuilder.AppendLine($@"// ----- Start Scripts For {resource.EntryType} ----");
                                localBuilder.AppendLine(fileContent ?? string.Empty);
                                localBuilder.AppendLine($"// ----- End Scripts For {resource.EntryType} ----");
                                localBuilder.AppendLine();
                            }
                        });

                    //Add batch result to the thread-safe collection.
                    if (localBuilder.Length > 0)
                        scriptChunks.Add(localBuilder.ToString());
                });

            // finaly, the content of all batches is combined by order.
            var finalBuilder = new StringBuilder();
            foreach (var chunk in scriptChunks)
                finalBuilder.AppendLine(chunk);

            _onProgress.Invoke($"Merged style resourcess for  {_module.ModuleName} module", 90);

            return finalBuilder.ToString();
        }
    }
}
