using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Tasks
{
    public interface ICustomTask
    {
        bool ContinueOnError { get; set; }
        void Start(ManifestModel manifest,IDataContext ctx, HttpContext httpContext);
    }
}
