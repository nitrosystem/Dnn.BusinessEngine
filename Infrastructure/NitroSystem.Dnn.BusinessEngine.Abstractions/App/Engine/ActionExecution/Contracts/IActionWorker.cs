using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IActionWorker
    {
        Task<ActionResult> CallAction(ActionExecutionContext context);
    }
}
