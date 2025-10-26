using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IActionExecutor
    {
        Task<ActionResult> ExecuteAsync(ActionExecutionContext context);
    }
}
