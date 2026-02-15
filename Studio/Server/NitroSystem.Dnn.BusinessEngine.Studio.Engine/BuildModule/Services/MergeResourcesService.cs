using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class MergeResourcesService : IMergeResourcesService
    {
        private ModuleDto _module;
        private Action<string, double> _onProgress;

        public async Task<(string Scripts, string Styles)> MergeResourcesAsync(ModuleDto module, int userId, IEnumerable<ModuleResourceDto> resources, Action<string, double> progress = null)
        {
            _module = module;
            _onProgress = progress;

            var resourcesLookup = resources.ToLookup(r => r.ResourceContentType);
            var scripts = await BuildScripts(resourcesLookup[ResourceContentType.Js]);
            var styles = await BuildStyles(resourcesLookup[ResourceContentType.Css]);

            //var results = await _workflow.ExecuteTasksAsync<object>(_module.Id.ToString(), userId,
            //   "BuildModuleWorkflow", "BuildModule", "MergeResourcesMiddleware", false,
            //   (Expression<Func<Task<string>>>)(() => BuildScripts(resourcesLookup[ResourceContentType.Js])),
            //   (Expression<Func<Task<string>>>)(() => BuildStyles(resourcesLookup[ResourceContentType.Css]))
            //  );
            //var scripts = results[0] as string;
            //var styles = results[1] as string;

            return (scripts, styles);
        }

        private async Task<string> BuildStyles(IEnumerable<ModuleResourceDto> resources)
        {
            var styles = new StringBuilder();
            var resourcesTypes = resources.ToLookup(r => r.IsContent);
            foreach (var resource in resourcesTypes[true])
            {
                styles.AppendLine($@"/* ----- Start Styles For {resource.EntryType} ---- */");
                styles.AppendLine(resource.Content);
                styles.AppendLine(Environment.NewLine);
            }

            var mergedStyles = await MergeStyleResources(resourcesTypes[false]);
            styles.AppendLine(mergedStyles);

            return styles.ToString();
        }

        private async Task<string> MergeStyleResources(IEnumerable<ModuleResourceDto> resources)
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
                    // هر batch برای خودش StringBuilder جدا داره
                    var localBuilder = new StringBuilder();

                    var items = batch
                        .GroupBy(r => r.ResourcePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesAsync(
                        items.Keys,
                        Constants.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var resource))
                            {
                                // append فقط روی localBuilder (ایمن در برابر ترد)
                                localBuilder.AppendLine($@"/* ----- Start Styles For {resource.EntryType} ---- */");
                                localBuilder.AppendLine(fileContent ?? string.Empty);
                                localBuilder.AppendLine($"/* ----- End Styles For {resource.EntryType} ----*/");
                                localBuilder.AppendLine();
                            }
                        });

                    // در پایان batch خروجی خودش را به مجموعهٔ thread-safe اضافه می‌کند
                    if (localBuilder.Length > 0)
                        scriptChunks.Add(localBuilder.ToString());
                });

            // در انتها محتوای همه batch‌ها با ترتیب تقریبی ترکیب می‌شود
            var finalBuilder = new StringBuilder();
            foreach (var chunk in scriptChunks)
                finalBuilder.AppendLine(chunk);

            _onProgress.Invoke($"Merged style resourcess for  {_module.ModuleName} module", 87.5);

            return finalBuilder.ToString();
        }

        private async Task<string> BuildScripts(IEnumerable<ModuleResourceDto> resources)
        {
            var scripts = new StringBuilder();
            var resourcesTypes = resources.ToLookup(r => r.IsContent);
            foreach (var resource in resourcesTypes[true])
            {
                scripts.AppendLine($@"/* ----- Start Scripts For {resource.EntryType} ---- */");
                scripts.AppendLine(resource.Content);
                scripts.AppendLine(Environment.NewLine);
            }

            var mergedStyles = await MergeScriptResources(resourcesTypes[false]);
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
                    // هر batch برای خودش StringBuilder جدا داره
                    var localBuilder = new StringBuilder();

                    var items = batch
                        .GroupBy(r => r.ResourcePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesAsync(
                        items.Keys,
                        Constants.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var resource))
                            {
                                // append فقط روی localBuilder (ایمن در برابر ترد)
                                localBuilder.AppendLine($@"// ----- Start Scripts For {resource.EntryType} ----");
                                localBuilder.AppendLine(fileContent ?? string.Empty);
                                localBuilder.AppendLine($"// ----- End Scripts For {resource.EntryType} ----");
                                localBuilder.AppendLine();
                            }
                        });

                    // در پایان batch خروجی خودش را به مجموعهٔ thread-safe اضافه می‌کند
                    if (localBuilder.Length > 0)
                        scriptChunks.Add(localBuilder.ToString());
                });

            // در انتها محتوای همه batch‌ها با ترتیب تقریبی ترکیب می‌شود
            var finalBuilder = new StringBuilder();

            foreach (var chunk in scriptChunks)
                finalBuilder.AppendLine(chunk);

            _onProgress.Invoke($"Merged style resourcess for  {_module.ModuleName} module", 90);

            return finalBuilder.ToString();
        }
    }
}
