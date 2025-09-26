using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using DotNetNuke.Entities.Portals;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IAction
    {
        Task<IActionResult> ExecuteAsync(ActionDto action, PortalSettings portalSettings);
    }
}
