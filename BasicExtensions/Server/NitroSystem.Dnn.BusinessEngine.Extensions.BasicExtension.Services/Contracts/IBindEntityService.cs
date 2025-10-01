using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
    public interface IBindEntityService
    {
        Task<object> GetBindEntityService(ActionDto action, PortalSettings portalSettings);
    }
}
