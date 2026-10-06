using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public interface IActionExecutor
    {
        Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData);
    }
}
