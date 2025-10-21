using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts
{
    public interface ILogMonitoringService
    {
        Task<IEnumerable<ScenarioDto>> QueryScenariosAsync(string processType = null, string status = null, string userId = null, int page = 1, int pageSize = 50);
        Task<ScenarioDto> GetScenarioAsync(Guid processId);
        Task<IEnumerable<StepDto>> GetStepsAsync(Guid processId);
        Task<MetricsDto> GetMetricsAsync();
        // methods used by producers (log framework) to push updates:
        Task NotifyStepAsync(StepDto step);
        Task NotifyScenarioUpsertAsync(ScenarioDto scenario);
    }
}
