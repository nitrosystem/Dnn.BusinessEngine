using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using System;
using System.Collections.Generic;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Components
{
    public abstract class ComponentBase : IComponent
    {
        #region Properties

        private bool disposed = false;

        public bool ContinueOnError { get; set; }

        public Queue<ICustomTask> Tasks { get; set; }

        #endregion

        #region Methods

        public abstract void CreateTasks();

        public virtual void Import(ManifestModel manifestModel,IDataContext ctx, HttpContext httpContext)
        {
            foreach (var task in this.Tasks)
            {
                try
                {
                    task.Start(manifestModel,ctx, httpContext);
                }
                catch (Exception ex)
                {
                    if (task.ContinueOnError) continue;
                    else throw ex;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }

                disposed = true;
            }
        }

        #endregion
    }
}
