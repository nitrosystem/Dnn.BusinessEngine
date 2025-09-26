using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts
{
   public interface ISubmitEntityService
    {
        Task<object> SaveEntityRow(ActionDto action, PortalSettings portalSettings);
    }
}
