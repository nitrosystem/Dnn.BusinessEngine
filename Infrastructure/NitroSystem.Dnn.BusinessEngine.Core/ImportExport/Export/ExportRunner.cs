using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public class ExportRunner
    {
        private readonly ISseNotifier _notifier;
        private readonly Func<IEnumerable<ExportComponent>> _componentResolver;
        private readonly ExportRequest _request;
        private readonly string _basePath;
        private readonly string _baseRelativePath;

        public ExportRunner(IServiceProvider serviceProvider, ExportRequest request, string basePath, string baseRelativePath)
        {
            _notifier = serviceProvider.GetRequiredService<ISseNotifier>();
            _componentResolver = serviceProvider.GetRequiredService<Func<IEnumerable<ExportComponent>>>();
            _request = request;
            _basePath = basePath;
            _baseRelativePath = baseRelativePath;
        }

        public async Task Export()
        {
            await _notifier.Publish(_request.Channel,
                new
                {
                    channel = _request.Channel,
                    type = "ActionCenter",
                    taskId = _request.Id,
                    icon = "codicon codicon-agent",
                    title = "Export Scenario...",
                    subtitle = "Export scenario full components",
                    message = $"Starting export scenario components",
                    percent = 0,
                }
            );

            var worker = new ExportWorker();
            worker.OnProgress += Export_OnProgress;
            worker.OnExportCompleted += Export_OnCompleted;

            var components = GetComponents();
            worker.RegisterComponents(components);

            var context = new ExportContext()
            {
                Scope = _request.ExportScope,
            };

            foreach (var item in _request.Params ?? new Dictionary<string, object>())
            {
                context.Set(item.Key, item.Value);
            }

            await worker.ExportAsync(context);
        }

        private IEnumerable<ExportComponent> GetComponents()
            => _componentResolver();

        #region Events

        private async Task Export_OnProgress(string componentName, bool isSuccess)
        {
            await _notifier.Publish(_request.Channel,
                new
                {
                    channel = _request.Channel,
                    type = "ActionCenter",
                    taskId = _request.Id,
                    message = $"Compoent {componentName} exporting is {(isSuccess ? "Successful" : "Failed")}",
                }
            );
        }

        private async Task Export_OnCompleted(ImportExportScope scope, List<ExportedItem> exportedItems)
        {
            var manifestModel = new
            {
                Scope = scope,
                Items = exportedItems,
                Version = _request.Version,
                DependencyVersion = "01.00.00"
            };
            var json = JsonConvert.SerializeObject(manifestModel);

            var relativePath = $@"{_baseRelativePath}business-engine/import-export/export/";
            var exportFolder = $@"{_basePath}business-engine\import-export\export\";
            var exportPath = $@"{_basePath}business-engine\import-export\export\{_request.Id}\";
            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            var jsonFile = exportPath + "export.json";
            FileUtil.CreateTextFile(jsonFile, json, true);

            var filename = StringHelper.ToKebabCase(_request.ExportName) + ".zip";
            var zipFile = exportFolder + filename;
            var result = ZipProvider.Zip(zipFile, exportPath, true);

            await _notifier.Publish(_request.Channel,
                new
                {
                    channel = _request.Channel,
                    type = "ActionCenter",
                    taskId = _request.Id,
                    message = $"{_request.ExportName} export has been successfully!.",
                    link = relativePath + filename,
                    percent = 100,
                    end = true,
                    close = false
                }
            );

            Directory.Delete(exportPath, true);
        }

        #endregion
    }
}
