using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser;
using NitroSystem.Dnn.BusinessEngine.Core.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class ResourceMachine : IResourceMachine
    {
        private readonly ICacheService _cacheService;
        private readonly IProgress<string> _progressHandler;

        private BuildModuleDto _buildModuleDto;
        private HashSet<string> _resources;

        public ResourceMachine(ICacheService cacheService, IProgress<string> progressHandler)
        {
            _cacheService = cacheService;
            _progressHandler = progressHandler;

            _resources = new HashSet<string>();
        }

        public async Task<List<ModuleResourceInfo>> RunAsync(IEnumerable<MachineResourceInfo> resources, HttpContext context)
        {
            var result = new List<ModuleResourceInfo>();

            var tasks = resources
                .OrderBy(r => r.Order)
                .Select(async resource =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(resource.Condition) || ParseCondition(resource.Condition))
                        {
                            if (resource.OperationType == OperationType.MergeResourceFiles)
                            {
                                var contentList = new Dictionary<(string entryType, string entryAdditional), string>();

                                await BatchExecutor.ExecuteInBatchesAsync(
                                    resource.ResourceFiles,
                                    batchSize: 5,
                                    async batch =>
                                    {
                                        foreach (var file in batch)
                                        {
                                            if (!string.IsNullOrEmpty(file.CacheKey))
                                            {
                                                file.CacheKey = file.CacheKey
                                                    .Replace("[FIELDTYPE]", file.EntryType)
                                                    .Replace("[FILENAME]", file.Additional);
                                            }

                                            var content = await LoadResourceFileAsync(file.FilePath, file.CacheKey, context);
                                            if (!string.IsNullOrWhiteSpace(content))
                                                content = Environment.NewLine + content + Environment.NewLine;
                                            contentList[(file.EntryType, file.Additional)] = content;
                                        }
                                    });

                                if (resource.MergeStrategy.MergedCallback != null)
                                {
                                    // _progressHandler.Report($"شروع مرج برای {resource.ResourceType} با استفاده از callback");

                                    var mergedContent = resource.MergeStrategy.MergedCallback.Invoke(new[] { contentList });
                                    await FileUtil.WriteFileContentAsync(resource.MergeStrategy.MergedOutputFilePath, mergedContent);
                                }
                                else
                                {
                                    // _progressHandler.Report($"مرج کردن فایل‌ها برای {resource.ResourceType}");

                                    var mergedContent = string.Join(Environment.NewLine, contentList.Select(c => c.Value));
                                    if (!string.IsNullOrWhiteSpace(mergedContent))
                                        await FileUtil.WriteFileContentAsync(resource.MergeStrategy.MergedOutputFilePath, mergedContent);
                                }

                                if (resource.AddToResources)
                                {
                                    string resourcePath = $"{resource.MergeStrategy.MergedResourcePath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")}";

                                    result.Add(new ModuleResourceInfo()
                                    {
                                        ModuleId = resource.ModuleId,
                                        ResourcePath = resourcePath,
                                        ResourceType = Path.GetExtension(resource.MergeStrategy.MergedOutputFilePath).Substring(1),
                                        LoadOrder = resource.Order,
                                    });
                                }

                                //_progressHandler.Report($"پایان مرج برای {resource.ResourceType}");
                            }
                            else
                            {
                                int i = 0;
                                foreach (var file in resource.ResourceFiles ?? Enumerable.Empty<MachineResourceFileInfo>())
                                {
                                    string resourcePath = file.FilePath;
                                    if (file.Additional == "GetResourcePathByMethod") resourcePath = GetDynamicResourcePath(resourcePath);

                                    if (resource.AddToResources)
                                    {
                                        result.Add(new ModuleResourceInfo()
                                        {
                                            ModuleId = resource.ModuleId,
                                            ResourcePath = resourcePath,
                                            ResourceType = Path.GetExtension(file.FilePath)?.TrimStart('.').ToLowerInvariant() ?? "none",
                                            LoadOrder = resource.Order + i++,
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //_progressHandler.Report($"خطا در مرج برای {resource.ResourceType}: {ex.Message}");
                        if (!resource.ContinueOnError)
                        {
                            throw ex;
                        }
                    }
                });

            await Task.WhenAll(tasks);

            return result;
        }

        private async Task<string> LoadResourceFileAsync(string resourcePath, string cacheKey, HttpContext context)
        {
            var result = string.IsNullOrEmpty(cacheKey) ? null : _cacheService.Get<string>(cacheKey);
            if (string.IsNullOrEmpty(result))
            {
                string fileMapPath = context.Server.MapPath($"~{resourcePath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions")}");
                if (!File.Exists(fileMapPath)) return string.Empty;

                result = await FileUtil.GetFileContentAsync(fileMapPath);

                if (!string.IsNullOrEmpty(cacheKey)) _cacheService.Set<string>(cacheKey, result);
            }

            return result;
        }

        private string GetDynamicResourcePath(string resourcePath)
        {
            var result = DynamicMethodExecutor.Execute(
                resourcePath,
                new object[] { _buildModuleDto }, // injectedParams
                typeof(ResourceFunctionProvider)
            );

            return result?.ToString() ?? string.Empty;
        }

        private bool ParseCondition(string condition)
        {
            var expressionTree = ConditionParser.Parse(condition);

            var param = Expression.Parameter(typeof(BuildModuleDto));
            var expression = expressionTree.BuildExpression(param);

            var lambda = Expression.Lambda<Func<BuildModuleDto, bool>>(expression, param).Compile();
            return lambda(_buildModuleDto);

            //return _ => true; // for return default lambda func
        }

        private async Task<IDictionary<string, (string ResourcePath, string Additional)>> LoadResourcesByType(IEnumerable<BuildModuleResourceDto> resources, ResourceType resourceType, string cacheKey, HttpContext context)
        {
            var result = await Task.WhenAll(
            resources
                .Where(r => r.ResourceType == resourceType)
                .Select(async r =>
                {
                    try
                    {
                        var content = await LoadResourceFileAsync(
                            r.ResourcePath,
                            (cacheKey ?? string.Empty).Replace("[FIELDTYPE]", r.EntryType),
                            context);
                        return new { r.EntryType, Content = content, r.Additional };
                    }
                    catch
                    {
                        return new { r.EntryType, Content = string.Empty, Additional = string.Empty };
                    }
                })
            );

            return result.ToDictionary(x => x.EntryType, x => (x.Content, x.Additional));
        }
    }

}
