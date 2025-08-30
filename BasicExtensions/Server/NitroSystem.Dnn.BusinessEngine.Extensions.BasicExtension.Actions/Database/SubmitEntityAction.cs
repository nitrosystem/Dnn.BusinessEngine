using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Database
{
    public class SubmitEntityAction : IAction
    {
        private readonly ISubmitEntityService _service;

        public SubmitEntityAction(ISubmitEntityService service)
        {
            _service = service;
        }

        public async Task<IActionResult> ExecuteAsync(ActionDto action, PortalSettings portalSettings)
        {
            IActionResult result = new ActionResult();

            try
            {
                var data = await _service.SaveEntityRow(action, portalSettings);

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
