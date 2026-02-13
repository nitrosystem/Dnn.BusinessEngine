using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models;
using NitroSystem.Dnn.BusinessEngine.Core.SseNotifier;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import
{
    public class ImportRunner
    {
        private readonly ISseNotifier _notifier;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Func<ImportExportScope, IEnumerable<ImportComponent>> _componentResolver;
        private readonly ImportRequest _request;

        public ImportRunner(IServiceProvider serviceProvider, ImportRequest request)
        {
            _notifier = serviceProvider.GetRequiredService<ISseNotifier>();
            _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            _componentResolver = serviceProvider.GetRequiredService<Func<ImportExportScope, IEnumerable<ImportComponent>>>();
            _request = request;
        }

        public async Task Import()
        {
            var json = await FileUtil.GetFileContentAsync(_request.ExportedFile);
            var exportedItems = JsonConvert.DeserializeObject<ImportableModel>(json);
            var worker = new ImportWorker();

            //worker.OnProgress += Export_OnProgress;
            //worker.OnExportCompleted += Export_OnCompleted;

            var components = GetComponents();
            worker.RegisterComponents(components);

            var context = new ImportContext()
            {
                Scope = _request.ImportScope,
            };

            foreach (var item in _request.Params ?? new Dictionary<string, object>())
            {
                context.Set(item.Key, item.Value);
            }

            await worker.ImportAsync(_unitOfWork, exportedItems.Items, context);
        }

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
            await _notifier.Publish(_request.Channel,
                new
                {
                    channel = _request.Channel,
                    type = "ActionCenter",
                    taskId = _request.Id,
                    message = $"Import has been successfully!.",
                    percent = 100,
                    end = true,
                    close = false
                }
            );
        }

        #endregion

        private IEnumerable<ImportComponent> GetComponents()
            => _componentResolver(_request.ImportScope);
    }
}
