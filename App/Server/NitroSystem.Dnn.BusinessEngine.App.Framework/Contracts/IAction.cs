using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IAction
    {
        Task<IActionResult> ExecuteAsync(ActionDto action, PortalSettings portalSettings);
    }
}
