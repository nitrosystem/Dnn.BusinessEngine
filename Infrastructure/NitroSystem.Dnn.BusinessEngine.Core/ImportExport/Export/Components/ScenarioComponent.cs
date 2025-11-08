using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export.Components
{
    public class ScenarioComponent : ComponentBase
    {
        private readonly IExportable _service;
        private readonly string _manifestPath;
        private readonly object[] _params;

        public ScenarioComponent(IExportable service, string manifestPath, params object[] args)
        {
            _service = service;
            _manifestPath = manifestPath;
            _params = args;

            this.Items = new List<string>();
            this.Values = new List<object>();
        }

        public override void CreateTasks()
        {
            var tasks = new Queue<ICustomTask>();

            var task1 = new SaveModelsAsJsonTask(this, _service, "GetScenarioViewModelAsync", _manifestPath, "ScenarioJsonFile", "scenario.json", _params);
            tasks.Enqueue(task1);

            //var task2 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Data.Repositories.GroupRepository, NitroSystem.Dnn.BusinessEngine.Data", "GetGroups", _manifestPath, "GroupsJsonFile", "groups.json", _params);
            //tasks.Enqueue(task2);

            //var task3 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.EntityMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetEntitiesViewModel", _manifestPath, "EntitiesJsonFile", "entities.json", _params);
            //tasks.Enqueue(task3);

            //var task4 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ViewModelMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetViewModelsViewModel", _manifestPath, "ViewModelsJsonFile", "view-models.json", _params);
            //tasks.Enqueue(task4);

            //var task5 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.DefinedListMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetListsViewModel", _manifestPath, "DefinedListsJsonFile", "defined-lists.json", _params);
            //tasks.Enqueue(task5);

            this.Tasks = tasks;
        }

        public override bool Export()
        {
            return base.Export();
        }
    }
}