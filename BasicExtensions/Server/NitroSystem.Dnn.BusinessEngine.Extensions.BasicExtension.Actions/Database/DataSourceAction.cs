using DotNetNuke.Entities.Portals;
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
    public class DataSourceAction : IAction
    {
        private readonly IDataSourceService _service;

        public DataSourceAction(IDataSourceService service)
        {
            _service = service;
        }

        public async Task<IActionResult> ExecuteAsync(ActionDto action, PortalSettings portalSettings)
        {
            IActionResult result = new ActionResult();

            try
            {
                var data = await _service.GetDataSourceService(action, portalSettings);
                var dict = new Dictionary<string, object>
                        {
                            { "Items", data.Items } ,
                            { "TotalCount",  data.TotalCount }
                        };

                result.Data = dict;
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
