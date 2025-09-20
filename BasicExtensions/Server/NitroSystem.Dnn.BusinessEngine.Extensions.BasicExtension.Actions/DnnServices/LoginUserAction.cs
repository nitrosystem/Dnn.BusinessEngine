using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models.DnnServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DnnServices;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.DnnServices
{
    public class LoginUserAction : IAction
    {
        private ILoginUserService _service;

        public LoginUserAction(ILoginUserService service)
        {
            _service = service;
        }

        public async Task<IActionResult> ExecuteAsync(ActionDto action, PortalSettings portalSettings)
        {
            await Task.Yield();

            return Login(action, portalSettings);
        }

        public IActionResult Execute(ActionDto action, PortalSettings portalSettings)
        {
            return Login(action, portalSettings);
        }

        private IActionResult Login(ActionDto action, PortalSettings portalSettings)
        {
            var username = action.Params.FirstOrDefault(p => p.ParamName == "@Username").ParamValue;
            var password = action.Params.FirstOrDefault(p => p.ParamName == "@Password").ParamValue;

            var status = _service.LoginUser((username ?? "").ToString(), (password ?? "").ToString(), portalSettings);

            return new ActionResult() { Data = status };
        }
    }
}
