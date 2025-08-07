using DotNetNuke.Abstractions.Portals;
using Newtonsoft.Json.Linq;
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
    public class BindEntityAction : IAction
    {
        private readonly IBindEntityService _service;

        public BindEntityAction(IBindEntityService service)
        {
            _service = service;
        }

        public async Task<IActionResult> ExecuteAsync(ActionDto action, IPortalSettings portalSettings)
        {
            IActionResult result = new ActionResult();

            try
            {
                var data = await _service.GetBindEntityService(action);

                result.Data = data != null
                    ? JToken.FromObject(data)
                    : null;

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
