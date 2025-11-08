using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Components;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
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
    public class SaveExtensionServiceTask : ICustomTask
    {
        private readonly string _manifestFolderPath;
        private object[] _params;

        public bool ContinueOnError { get; set; }

        public SaveExtensionServiceTask(string manifestFolderPath, params object[] args)
        {
            _manifestFolderPath = manifestFolderPath;
            _params = args;
        }

        Func<string, string> openFileFunc = FileUtil.GetFileContent;

        private void ProcessExternalServices(string[] jsonFiles, IDataContext ctx, HttpContext httpContext)
        {
            _params = _params.Append(ctx).ToArray();
            _params = _params.Append(httpContext).ToArray();

            object[] methodParams = (object[])_params.Clone();

            var items = ExtensionExportItemRepository.Instance.GetExportItemsByType(Data.Enums.OperationType.Import, "Service");
            foreach (var jsonFile in jsonFiles)
            {
                var json = openFileFunc(_manifestFolderPath + jsonFile);

                var item = items.FirstOrDefault(s => s.ExtensionName.ToLower() == jsonFile.Replace(".json", ""));

                methodParams = methodParams.Prepend(json).ToArray();

                ReflectionUtil.CallMethod<object>(item.BusinessControllerClass, item.MethodName, methodParams);
            }
        }

        public void Start(ManifestModel manifest, IDataContext ctx, HttpContext httpContext)
        {
            ProcessExternalServices(manifest.ExtensionServices, ctx, httpContext);
        }
    }
}

