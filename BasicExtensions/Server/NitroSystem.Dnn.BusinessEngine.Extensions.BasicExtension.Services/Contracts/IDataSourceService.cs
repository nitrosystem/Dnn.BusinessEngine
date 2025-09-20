using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
    public interface IDataSourceService
    {
        Task<(IEnumerable<object> Items, int? TotalCount)> GetDataSourceServiceAsync(ActionDto action, PortalSettings portalSettings);
        (IEnumerable<object> Items, int? TotalCount) GetDataSourceService(ActionDto action, PortalSettings portalSettings);
    }
}
