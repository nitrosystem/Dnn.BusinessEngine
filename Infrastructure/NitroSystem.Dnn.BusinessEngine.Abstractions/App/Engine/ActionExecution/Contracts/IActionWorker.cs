using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.DContractsto
{
    public interface IActionWorker
    {
        Task<ActionResult> CallAction(ActionExecutionContext context);
    }
}
