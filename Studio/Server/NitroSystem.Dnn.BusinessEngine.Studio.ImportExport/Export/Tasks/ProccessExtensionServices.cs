using DotNetNuke.Common.Utilities;
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

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Tasks
{
    public class ProccessExtensionServices : ICustomTask
    {
        private readonly IComponent _component;
        private readonly string _manifestPath;
        private readonly object[] _params;

        public bool ContinueOnError { get; set; }

        Func<string, string, bool, bool> createFile = FileUtil.CreateTextFile;

        public ProccessExtensionServices(IComponent component, string manifestPath, string manifestPropertyName, params object[] args)
        {
            _component = component;
            _manifestPath = manifestPath;
            _params = args;

            component.Items.Add(manifestPropertyName);
        }

        private void ProcessExternalServices()
        {
            var items = ExtensionExportItemRepository.Instance.GetExportItemsByType(Data.Enums.OperationType.Export, "Service").ToHashSet<ExtensionImportExportItemInfo>();
            CallbackServiceMethods(items);
        }

        private void CallbackServiceMethods(HashSet<ExtensionImportExportItemInfo> items)
        {
            var instances = new Dictionary<string, object>();

            var groups = items.GroupBy(s => s.BusinessControllerClass);

            foreach (var group in groups)
            {
                string className = group.Key;  // BusinessControllerClass
                List<string> values = new List<string>();

                foreach (var extension in group)
                {
                    var data = ReflectionUtil.CallMethod<string>(className, extension.MethodName, _params);
                    var filename = extension.ExtensionName.ToLower() + ".json";
                    this.createFile(_manifestPath + filename, (string)data, true);

                    values.Add(filename);

                    _component.Values.Add(values.ToArray<string>());
                }
            }
        }

        public void Start()
        {
            ProcessExternalServices();
        }
    }
}

