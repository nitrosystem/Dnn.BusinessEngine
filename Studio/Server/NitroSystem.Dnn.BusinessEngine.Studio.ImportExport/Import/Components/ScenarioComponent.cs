using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import.Components
{
    public class ScenarioComponent : ComponentBase
    {
        private readonly string _manifestFolderPath;
        private readonly object[] _params;
        public ScenarioComponent(string manifestFolderPath, params object[] args)
        {
            _manifestFolderPath = manifestFolderPath;
            _params = args;
        }

        public override void CreateTasks()
        {
            var tasks = new Queue<ICustomTask>();

            var task1 = new SaveDataTask("NitroSystem.Dnn.BusinessEngine.Api.Controllers.ScenarioController, NitroSystem.Dnn.BusinessEngine.Api", "ImportScenario", "ScenarioJsonFile", _manifestFolderPath, _params);
            tasks.Enqueue(task1);

            var task2 = new SaveDataTask("NitroSystem.Dnn.BusinessEngine.Api.Controllers.GroupController, NitroSystem.Dnn.BusinessEngine.Api", "ImportGroup", "GroupsJsonFile", _manifestFolderPath, _params);
            tasks.Enqueue(task2);

            var task3 = new SaveDataTask("NitroSystem.Dnn.BusinessEngine.Api.Controllers.EntityController, NitroSystem.Dnn.BusinessEngine.Api", "ImportEntities", "EntitiesJsonFile", _manifestFolderPath, _params);
            tasks.Enqueue(task3);

            var task4 = new SaveDataTask("NitroSystem.Dnn.BusinessEngine.Api.Controllers.ViewModelController, NitroSystem.Dnn.BusinessEngine.Api", "ImportViewModels", "ViewModelsJsonFile", _manifestFolderPath, _params);
            tasks.Enqueue(task4);

            this.Tasks = tasks;
        }

        public override void Import(ManifestModel manifestModel, IDataContext ctx, HttpContext httpContext)
        {
            base.Import(manifestModel, ctx, httpContext);
        }
    }
}