using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution.Contracts
{
    public interface IActionExecutor
    {
        Task<ActionResult> ExecuteAsync(IEngineContext context);
    }
}
