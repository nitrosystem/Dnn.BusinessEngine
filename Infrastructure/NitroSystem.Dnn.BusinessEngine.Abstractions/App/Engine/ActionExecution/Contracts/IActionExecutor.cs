using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Models;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.DContractsto
{
    public interface IActionExecutor
    {
        Task<ActionResult> ExecuteAsync(ActionExecutionContext context);
    }
}
