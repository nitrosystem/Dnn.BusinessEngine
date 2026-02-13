using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Events;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Components
{
    public abstract class ComponentBase : IComponent
    {
        private bool _disposed = false;

        #region Properties

        public event ExportProgressHandler OnProgress;

        public string Name { get; set; }

        public string Data { get; set; }

        public IDictionary<string, string> Resources { get; set; }

        public Queue<ICustomTask> Tasks { get; set; } = new Queue<ICustomTask>();

        #endregion

        #region Methods

        public abstract void CreateTasks();

        public async virtual Task<bool> ExportComponentAsync()
        {
            foreach (var task in Tasks)
            {
                try
                {
                    var result = await task.ExecuteAsync();

                    OnProgress?.Invoke(Name, task.Name, result.IsSuccess);

                    if (result.IsSuccess)
                    {
                        Data = result.Data;
                        Resources = result.Resources;
                    }
                }
                catch (Exception ex)
                {
                    if (task.ContinueOnError) continue;
                    else return false;
                }
            }

            return true;
        }

        //public void CompleteComponentExport(ManifestModel manifest)
        //{
        //    var dictionary = this.Items.Zip(this.Values, (k, v) => new { Key = k, Value = v })
        //                         .ToDictionary(x => x.Key, x => x.Value);

        //    foreach (var item in dictionary)
        //    {
        //        manifest.SetPropertyValueByName(item.Key, item.Value);
        //    }
        //}

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
