using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Events;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import
{
    public class ImportWorker
    {
        private readonly List<ImportComponent> _components = new List<ImportComponent>();

        public event ImportProgressHandler OnProgress;
        public event ImportCompletedHandler OnImportCompleted;

        public void RegisterComponents(IEnumerable<ImportComponent> components)
        {
            foreach (var component in components)
            {
                _components.Add(component);
            }
        }

        public async Task ImportAsync(IUnitOfWork unitOfWork, IEnumerable<ExportedItem> items, ImportContext context)
        {
            unitOfWork.BeginTransaction();

            try
            {
                context.UnitOfWork = unitOfWork;
                context.DataTrack = new Dictionary<string, object>();

                foreach (var component in _components)
                {
                    var item = items.FirstOrDefault(i => i.ComponentName == component.Name);
                    if (item != null)
                    {
                        var result = await component.Service.ImportAsync(item.ExportedJson, context);

                        await Task.Delay(150);
                        OnProgress?.Invoke(component.Name, result.IsSuccess);
                    }
                }
            }
            catch (Exception ex)
            {
                unitOfWork.Rollback();
                throw;
            }

            unitOfWork.Commit();

            await Task.Delay(150);
            OnImportCompleted?.Invoke(context);
        }
    }
}
