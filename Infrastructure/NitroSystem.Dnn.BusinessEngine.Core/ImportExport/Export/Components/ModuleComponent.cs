using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Import_Export.Enums;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Export.Components
{
    public class ModuleComponent : ComponentBase
    {
        private readonly PortalSettings _portalSettings;
        private readonly string _manifestPath;
        private readonly string _scenarioName;
        private readonly object[] _params;
        public ModuleComponent(PortalSettings portalSettings, string manifestPath, string scenarioName, params object[] args)
        {
            this.Items = new List<string>();
            this.Values = new List<object>();

            _portalSettings = portalSettings;
            _manifestPath = manifestPath;
            _scenarioName = scenarioName;
            _params = args;
        }

        public override void CreateTasks()
        {
            var tasks = new Queue<ICustomTask>();

            var task1 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ModuleMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetModulesViewModel", _manifestPath, "ModulesJsonFile", "modules.json", _params);
            tasks.Enqueue(task1);

            var task2 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ModuleVariableMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetScenarioVariablesViewModel", _manifestPath, "ModuleVariablesJsonFile", "modules-variables.json", _params);
            tasks.Enqueue(task2);

            var task3 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ModuleFieldMappings, NitroSystem.Dnn.BusinessEngine.Api", "GetFieldsViewModel", _manifestPath, "ModulesFieldsJsonFile", "modules-fields.json", _params);
            tasks.Enqueue(task3);

            var task4 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.ActionMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetActionsViewModel", _manifestPath, "ActionsJsonFile", "actions.json", _params);
            tasks.Enqueue(task4);

            var task5 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Data.Repositories.ModuleFieldSettingRepository, NitroSystem.Dnn.BusinessEngine.Data", "GetScenarioFieldSettings", _manifestPath, "ModulesFieldsSettingsJsonFile", "modules-fields-settings.json", _params);
            tasks.Enqueue(task5);

            var task6 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Data.Repositories.PageResourceRepository, NitroSystem.Dnn.BusinessEngine.Data", "GetScenarioResources", _manifestPath, "PageResourcesJsonFile", "pages-resources.json", _params);
            tasks.Enqueue(task6);

            string sourcePath = _portalSettings.HomeSystemDirectoryMapPath + $@"business-engine\{_scenarioName}";
            var task7 = new ZipTask(this,_manifestPath , "_scenario-folder.zip", "ScenarioFolderZipFile" ,sourcePath);
            tasks.Enqueue(task7);

            this.Tasks = tasks;
        }

        public override bool Export()
        {
            return base.Export();
        }
    }
}