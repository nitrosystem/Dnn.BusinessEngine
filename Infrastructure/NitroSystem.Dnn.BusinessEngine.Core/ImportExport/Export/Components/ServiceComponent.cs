using DotNetNuke.ComponentModel;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Components
{
    public class ServiceComponent : ComponentBase
    {
        private readonly string _manifestPath;
        private readonly object[] _params;
        public ServiceComponent(string manifestPath, params object[] args)
        {
            this.Items = new List<string>();
            this.Values = new List<object>();

            _manifestPath = manifestPath;
            _params = args;
        }

        public override void CreateTasks()
        {
            var tasks = new Queue<ICustomTask>();

            var task1 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ServiceMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetServicesViewModel", _manifestPath, "ServicesJsonFile", "services.json", _params);
            tasks.Enqueue(task1);

            var task2 = new ProccessExtensionServices(this,_manifestPath, "ExtensionServices", _params);
            tasks.Enqueue(task2);

            this.Tasks = tasks;
        }

        public override bool Export()
        {
            return base.Export();
        }
    }
}