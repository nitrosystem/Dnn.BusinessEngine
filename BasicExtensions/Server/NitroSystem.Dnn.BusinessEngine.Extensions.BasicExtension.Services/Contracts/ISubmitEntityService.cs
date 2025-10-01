using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
   public interface ISubmitEntityService
    {
        Task<object> SaveEntityRow(ActionDto action, PortalSettings portalSettings);
    }
}
