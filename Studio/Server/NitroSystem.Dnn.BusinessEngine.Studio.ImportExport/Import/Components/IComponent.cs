using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Data;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Components
{
    public interface IComponent : IDisposable
    {
        bool ContinueOnError { get; set; }

        Queue<ICustomTask> Tasks { get; set; }

        void CreateTasks();

        void Import(ManifestModel manifestModel, IDataContext ctx, HttpContext httpContext);
    }
}
