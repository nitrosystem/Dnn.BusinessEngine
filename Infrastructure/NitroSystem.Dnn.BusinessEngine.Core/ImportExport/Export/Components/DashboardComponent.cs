using DotNetNuke.ComponentModel;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
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
    public class DashboardComponent : ComponentBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly string _manifestPath;
        private readonly object[] _params;
        public DashboardComponent(IDashboardService dashboardService, string manifestPath, params object[] args)
        {
            _dashboardService = dashboardService;
            _manifestPath = manifestPath;
            _params = args;

            base.Items = new List<string>();
            base.Values = new List<object>();

        }

        public override void CreateTasks()
        {
            var tasks = new Queue<ICustomTask>();

            var task1 = new SaveModelsAsJsonTask(this, _dashboardService, "GetDashboardsViewModel", _manifestPath, "DashboardsJsonFile", "dashboards.json", _params);
            tasks.Enqueue(task1);

            var task2 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Api.Mapping.DashboardMapping, NitroSystem.Dnn.BusinessEngine.Api", "GetDashboardPagesViewModel", _manifestPath, "DashboardPagesJsonFile", "dashboards-pages.json", _params);
            tasks.Enqueue(task2);

            var task3 = new SaveModelsAsJsonTask(this, "NitroSystem.Dnn.BusinessEngine.Data.Repositories.DashboardPageModuleRepository, NitroSystem.Dnn.BusinessEngine.Data", "GetScenarioModules", _manifestPath, "DashboardPagesModulesJsonFile", "dashboards-pages-modules.json", _params);
            tasks.Enqueue(task3);

            this.Tasks = tasks;
        }

        public override bool Export()
        {
            return base.Export();
        }
    }
}