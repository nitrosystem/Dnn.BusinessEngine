using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Components;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Tasks
{
    public class SaveDataTask : ICustomTask
    {
        private readonly string _className;
        private readonly string _methodName;
        private readonly string _manifestPropertyName;
        private readonly string _manifestFolderPath;
        private object[] _params;

        public bool ContinueOnError { get; set; }

        public SaveDataTask(string className, string methodName, string manifestPropertyName, string manifestFolderPath, params object[] args)
        {
            _className = className;
            _methodName = methodName;
            _manifestPropertyName = manifestPropertyName;
            _manifestFolderPath = manifestFolderPath;
            _params = args;
        }

        public void SendData(string json, IDataContext ctx, HttpContext httpContext)
        {
            _params = _params.Prepend(json).ToArray();
            _params = _params.Append(ctx).ToArray();
            _params = _params.Append(httpContext).ToArray();

            ReflectionUtil.CallMethod<object>(_className, _methodName, _params);
        }

        public void Start(ManifestModel manifest, IDataContext ctx, HttpContext httpContext)
        {
            var jsonFile = manifest.GetPropertyValueByName(_manifestPropertyName);
            var json = FileUtil.GetFileContent(_manifestFolderPath + jsonFile);

            SendData(json, ctx, httpContext);
        }
    }
}
