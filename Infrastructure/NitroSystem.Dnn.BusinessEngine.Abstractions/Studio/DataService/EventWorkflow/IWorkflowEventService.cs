using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Workflow
{
   public interface IWorkflowEventService
    {
        Task SaveTaskBatchAsync(List<WorkflowEventTaskDto> tasks);
    }
}
