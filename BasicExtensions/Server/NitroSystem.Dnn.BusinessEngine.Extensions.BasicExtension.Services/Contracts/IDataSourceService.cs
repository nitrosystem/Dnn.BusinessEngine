using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
    public interface IDataSourceService
    {
        Task<(IEnumerable<object> Items, int? TotalCount)> GetDataSourceService(ActionDto action, PortalSettings portalSettings);
    }
}
