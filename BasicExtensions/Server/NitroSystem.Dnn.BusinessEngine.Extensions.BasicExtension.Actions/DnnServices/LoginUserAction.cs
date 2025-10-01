using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;

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

            var username = action.Params.FirstOrDefault(p => p.ParamName == "@Username").ParamValue;
            var password = action.Params.FirstOrDefault(p => p.ParamName == "@Password").ParamValue;

            var status = _service.LoginUser((username ?? "").ToString(), (password ?? "").ToString(), portalSettings);

            return new ActionResult() { Data = status };
        }
    }
}
