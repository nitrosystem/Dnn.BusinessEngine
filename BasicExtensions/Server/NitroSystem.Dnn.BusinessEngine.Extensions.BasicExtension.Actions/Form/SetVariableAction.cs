using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Enums;
using DotNetNuke.Abstractions.Portals;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Actions.Form
{
    public class SetVariableAction : IAction
    {
        public async Task<IActionResult> ExecuteAsync(ActionDto action, IPortalSettings portalSettings)
        {
            await Task.Yield();

            return new ActionResult() { ResultStatus = ActionResultStatus.Successful };
        }
    }
}
