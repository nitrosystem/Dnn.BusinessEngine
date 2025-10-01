using System;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Database
{
    public class BindEntityAction : IAction
    {
        private readonly IBindEntityService _service;

        public BindEntityAction(IBindEntityService service)
        {
            _service = service;
        }

        public async Task<IActionResult> ExecuteAsync(ActionDto action, PortalSettings portalSettings)
        {
            IActionResult result = new ActionResult();

            try
            {
                var data = await _service.GetBindEntityService(action, portalSettings);

                result.Data = data;
                result.ResultStatus = ActionResultStatus.Successful;
            }
            catch (Exception ex)
            {
                result.ResultStatus = ActionResultStatus.Error;
                result.ErrorException = ex;
            }

            return result;
        }
    }
}
