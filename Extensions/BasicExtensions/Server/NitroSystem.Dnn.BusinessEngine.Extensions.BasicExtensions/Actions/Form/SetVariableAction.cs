using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Form
{
    public class SetVariableAction : IActionExecutor
    {
        public async Task<object> ExecuteAsync(ActionDto action, ConcurrentDictionary<string, object> moduleData)
        {
            await Task.Yield();
            
            return true;
        }
    }
}
