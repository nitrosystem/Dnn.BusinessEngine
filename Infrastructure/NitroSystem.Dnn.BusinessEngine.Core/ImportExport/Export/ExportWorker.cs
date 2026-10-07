using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Events;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export
{
    public class ExportWorker
    {
        private readonly List<ExportComponent> _components = new List<ExportComponent>();
        private readonly List<ExportedItem> _exportedItems = new List<ExportedItem>();

        public event ExportProgressHandler OnProgress;
        public event ExportCompletedHandler OnExportCompleted;

        public void RegisterComponents(IEnumerable<ExportComponent> components)
        {
            foreach (var component in components)
            {
                _components.Add(component);
            }
        }

        public async Task ExportAsync(ExportContext context)
        {
            foreach (var component in _components)
            {
                var result = await component.Service.ExportAsync(context);
                if (result != null)
                {
                    var json = JsonConvert.SerializeObject(result.Result);

                    _exportedItems.Add(new ExportedItem(component.Name, json));

                    OnProgress?.Invoke(component.Name, result.IsSuccess);
                }
            }

            OnExportCompleted?.Invoke(context.Scope, _exportedItems);
        }
    }
}