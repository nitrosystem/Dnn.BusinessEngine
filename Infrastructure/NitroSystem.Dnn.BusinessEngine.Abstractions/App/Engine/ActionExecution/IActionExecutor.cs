using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.ActionExecution
{
    public interface IActionExecutor
    {
        Task<object> ExecuteAsync(ActionDto action, string basePath);
    }
}
