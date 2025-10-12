using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using System.Text;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Services
{
    public class MergeResourcesService : IMergeResourcesService
    {
        public async Task<(string Scripts, string Styles)> MergeResourcesAsync(IEnumerable<ModuleResourceDto> resources)
        {
            var resourcesLookup = resources.ToLookup(r => r.ResourceContentType);
            var scripts = await MergeScriptResources(resourcesLookup[ModuleResourceContentType.Js]);
            var styles = await MergeStyleResources(resourcesLookup[ModuleResourceContentType.Css]);
            
            return (scripts,styles);
        }

        private async Task<string> MergeScriptResources(IEnumerable<ModuleResourceDto> resources)
        {
            var scripts = new StringBuilder();

            await ParallelBatchExecutor.ExecuteInParallelBatchesAsync(
                resources,
                batchSize: 5,
                maxDegreeOfParallelism: 3,
                async batch =>
                {
                    var items = batch
                        .GroupBy(r => r.ResourcePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesWithCachingAsync(
                        items.Keys,
                        Constants.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var resource))
                            {
                                scripts.AppendLine($@"// ----- Start Script For {resource.EntryType} ----");
                                scripts.AppendLine(fileContent);
                                scripts.AppendLine(Environment.NewLine);
                            }
                        });
                });

            return scripts.ToString();
        }

        private async Task<string> MergeStyleResources(IEnumerable<ModuleResourceDto> resources)
        {
            var styles = new StringBuilder();
            var resourcesTypes = resources.ToLookup(r => r.IsContent);
            foreach (var resource in resourcesTypes[true])
            {
                styles.AppendLine($@"/* ----- Start Styles For {resource.EntryType} ---- */");
                styles.AppendLine(resource.Content);
                styles.AppendLine(Environment.NewLine);
            }

            await ParallelBatchExecutor.ExecuteInParallelBatchesAsync(
                resourcesTypes[false],
                batchSize: 5,
                maxDegreeOfParallelism: 3,
                async batch =>
                {
                    var items = batch
                        .GroupBy(r => r.ResourcePath?.ReplaceFrequentTokens())
                        .ToDictionary(
                            g => g.Key,
                            g => g.First()
                        );

                    await FileUtil.LoadFilesWithCachingAsync(
                        items.Keys,
                        Constants.MapPath,
                        (itemKey, fileContent) =>
                        {
                            if (items.TryGetValue(itemKey, out var resource))
                            {
                                styles.AppendLine($@"/* ----- Start Script For {resource.EntryType} ---- */");
                                styles.AppendLine(fileContent);
                            }
                        });
                });

            return styles.ToString();
        }
    }
}
