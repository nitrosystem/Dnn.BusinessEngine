using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser;
using NitroSystem.Dnn.BusinessEngine.Core.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public class ResourceMachine : IResourceMachine
    {
        private readonly ICacheService _cacheService;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);

        private BuildModuleDto _buildModuleDto;

        public ResourceMachine(ICacheService cacheService)
        {
            _cacheService = cacheService;
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
                            if (resource.OperationType == OperationType.MergeResourceFiles && resource.MergeStrategy != null)
                            {
                                var contentList = new Dictionary<(string entryType, string entryAdditional), string>();

                                if (!resource.MergeStrategy.IgnoreLoadingResourceFiles)
                                {
                                    await BatchExecutor.ExecuteInBatchesAsync(
                                      resource.ResourceFiles,
                                      batchSize: 5,
                                      async batch =>
                                      {
                                          // تنظیم نهایی کلید کش
                                          foreach (var file in batch)
                                          {
                                              if (!string.IsNullOrEmpty(file.CacheKey))
                                              {
                                                  file.CacheKey = file.CacheKey
                                                      .Replace("[FIELDTYPE]", file.EntryType)
                                                      .Replace("[FILENAME]", file.Additional);
                                              }
                                          }

                                          // لود موازی فایل‌ها (محدود به 5 فایل همزمان)
                                          var pathToEntryMap = batch.ToDictionary(
                                              f => f.ResourcePath.Replace("[EXTPATH]", "/DesktopModules/BusinessEngine/extensions"),
                                              f => (f.EntryType, f.Additional, f.CacheKey));

                                          var contents = await LoadMultipleFilesAsync(
                                              resourcePaths: pathToEntryMap.Keys.ToList(),
                                              count: 5, // همزمانی
                                              cacheService: _cacheService,
                                              context: context
                                          );

                                          // نتیجه را وارد contentList کن
                                          foreach (var kvp in contents)
                                          {
                                              var path = kvp.Key;
                                              var content = kvp.Value;

                                              if (!string.IsNullOrWhiteSpace(content))
                                                  content = Environment.NewLine + content + Environment.NewLine;

                                              var meta = pathToEntryMap[path];
                                              contentList[(meta.EntryType, meta.Additional)] = content;
                                          }
                                      });
                                }

                                if (resource.MergeStrategy.MergedCallback != null)
                                {
                                    var mergedContent = resource.MergeStrategy.MergedCallback.Invoke(new[] { contentList });
                                    await FileUtil.WriteFileContentAsync(resource.MergeStrategy.MergedOutputFilePath, mergedContent);
                                }
                                else
                                {
                                    var mergedContent = string.Join(Environment.NewLine, contentList.Select(c => c.Value));
                                    if (!string.IsNullOrWhiteSpace(mergedContent))
                                        await FileUtil.WriteFileContentAsync(resource.MergeStrategy.MergedOutputFilePath, mergedContent);
                                }

                                if (resource.AddToResources && !string.IsNullOrEmpty(resource.MergeStrategy.MergedResourcePath))
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
                            }
                            else if (resource.OperationType == OperationType.AddResourcePathToModuleResources)
                            {
                                int i = 0;
                                foreach (var file in resource.ResourceFiles ?? Enumerable.Empty<MachineResourceFileInfo>())
                                {
                                    string resourcePath = file.ResourcePath;
                                    if (file.Additional == "GetResourcePathByMethod") resourcePath = GetDynamicResourcePath(resourcePath);

                                    if (resource.AddToResources)
                                    {
                                        result.Add(new ModuleResourceInfo()
                                        {
                                            ModuleId = resource.ModuleId,
                                            ResourcePath = resourcePath,
                                            ResourceType = Path.GetExtension(file.ResourcePath)?.TrimStart('.').ToLowerInvariant() ?? "none",
                                            LoadOrder = resource.Order + i++,
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!resource.ContinueOnError)
                        {
                            throw ex;
                        }
                    }
                });

            await Task.WhenAll(tasks);

            return result;
        }

        public async Task<Dictionary<string, string>> LoadMultipleFilesAsync(
           List<string> resourcePaths,
           int count,
           ICacheService cacheService,
           HttpContext context)
        {
            var result = new Dictionary<string, string>();
            var tasks = new List<Task>();

            foreach (var path in resourcePaths)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        string cacheKey = $"resource:{path.Replace("/", "_")}";
                        string content = string.IsNullOrEmpty(cacheKey) ? null : cacheService.Get<string>(cacheKey);

                        if (string.IsNullOrEmpty(content))
                        {
                            string fileMapPath = context.Server.MapPath($"~{path}");
                            if (File.Exists(fileMapPath))
                            {
                                content = await FileUtil.GetFileContentAsync(fileMapPath);
                                if (!string.IsNullOrEmpty(cacheKey))
                                {
                                    cacheService.Set<string>(cacheKey, content);
                                }
                            }
                            else
                            {
                                content = string.Empty;
                            }
                        }

                        lock (result)
                        {
                            result[path] = content;
                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return result;
        }

        private async Task<string> LoadResourceFileAsync(string resourcePath, string cacheKey, HttpContext context)
        {
            var result = string.IsNullOrEmpty(cacheKey) ? null : _cacheService.Get<string>(cacheKey);
            if (string.IsNullOrEmpty(result))
            {
                string fileMapPath = context.Server.MapPath($"~{resourcePath}");
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

        }
    }
}
