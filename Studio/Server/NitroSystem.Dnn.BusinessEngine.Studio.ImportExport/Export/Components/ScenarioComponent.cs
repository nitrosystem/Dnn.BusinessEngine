using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ImportExport.Export.Components
{
    public class ScenarioComponent : ComponentBase
    {
        private readonly IExportable _service;
        private readonly object[] _params;

        public ScenarioComponent(IExportable service, params object[] args)
        {
            _service = service;
            _params = args;

            Name = "Scenario";
        }

        public override void CreateTasks()
        {
            var task = new GetModelsAsJsonTask<ScenarioViewModel>(_service, "GetScenarioViewModelAsync", _params);
            Tasks.Enqueue(task);

            //var task2 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Data.Repositories.GroupRepository, NitroSystem.Dnn.BusinessEngine.Data", "GetGroups", _manifestPath, "GroupsJsonFile", "groups.json", _params);
            //tasks.Enqueue(task2);

            //var task3 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.EntityMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetEntitiesViewModel", _manifestPath, "EntitiesJsonFile", "entities.json", _params);
            //tasks.Enqueue(task3);

            //var task4 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ViewModelMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetViewModelsViewModel", _manifestPath, "ViewModelsJsonFile", "view-models.json", _params);
            //tasks.Enqueue(task4);

            //var task5 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.DefinedListMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetListsViewModel", _manifestPath, "DefinedListsJsonFile", "defined-lists.json", _params);
            //tasks.Enqueue(task5);
        }
    }
}