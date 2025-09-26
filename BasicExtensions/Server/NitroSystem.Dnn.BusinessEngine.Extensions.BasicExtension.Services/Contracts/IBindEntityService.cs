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
    public interface IBindEntityService
    {
        Task<object> GetBindEntityService(ActionDto action, PortalSettings portalSettings);
    }
}
