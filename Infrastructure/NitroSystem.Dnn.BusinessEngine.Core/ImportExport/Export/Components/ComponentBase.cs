using System;
using System.Linq;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export.Components
{
    public abstract class ComponentBase : IComponent
    {
        private bool _disposed = false;

        #region Properties

        public bool ContinueOnError { get; set; }

        public List<string> Items { get; set; }

        public List<object> Values { get; set; }

        public Queue<ICustomTask> Tasks { get; set; }

        #endregion

        #region Methods

        public abstract void CreateTasks();

        public virtual bool Export()
        {
            foreach (var task in this.Tasks)
            {
                try
                {
                    task.Start();
                }
                catch (Exception ex)
                {
                    if (task.ContinueOnError) continue;
                    else return false;
                }
            }

            return true;
        }

        public void CompleteComponentExport(ManifestModel manifest)
        {
            var dictionary = this.Items.Zip(this.Values, (k, v) => new { Key = k, Value = v })
                                 .ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in dictionary)
            {
                manifest.SetPropertyValueByName(item.Key, item.Value);
            }
        }

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
