using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Api.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.IO;
using System.Drawing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System.IO.Compression;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.App.Api
{
    public class ModuleController : DnnApiController
    {
        private readonly IUserDataStore _userDataStore;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly IActionWorker _actionWorker;

        public ModuleController(
            IUserDataStore userDataStore,
            IModuleService moduleService,
            IActionService actionService,
            IActionWorker actionWorker
        )
        {
            _userDataStore = userDataStore;
            _moduleService = moduleService;
            _actionService = actionService;
            _actionWorker = actionWorker;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetModule(Guid moduleId, string connectionId, string pageUrl)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId);
                if (module == null) throw new Exception("Module Not Config");

                var moduleData = await _userDataStore.GetOrCreateModuleDataAsync(connectionId, module.Id, PortalSettings);
                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(pageUrl);

                await _actionWorker.CallActions(moduleData, moduleId, null, "OnPageLoad", PortalSettings);

                var data = _userDataStore.GetDataForClients(moduleId, moduleData);

                var variables = await _moduleService.GetModuleVariables(moduleId, Services.Enums.ModuleVariableScope.Global);

                var fields = await _moduleService.GetFieldsViewModelAsync(moduleId);
                var actions = await _actionService.GetActionsDtoForClientAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    fields,
                    actions,
                    data,
                    variables
                });

                //return Request.CreateResponse(HttpStatusCode.OK, new
                //{
                //    mf = ProtectPayload(fields),
                //    ma = ProtectPayload(actions),
                //    md = ProtectPayload(data)
                //});
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> CallAction(ActionDto action)
        {
            try
            {
                var moduleData = await _userDataStore.UpdateModuleData(
                    action.ConnectionId,
                    action.ModuleId,
                    action.Data,
                    PortalSettings
                );

                moduleData["_PageParam"] = UrlHelper.ParsePageParameters(action.PageUrl);

                await _actionWorker.CallActions(action.ActionIds, moduleData, PortalSettings);

                var data = _userDataStore.GetDataForClients(action.ModuleId, moduleData);

                return Request.CreateResponse(HttpStatusCode.OK, new { data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public HttpResponseMessage PingConnection(ActionDto user)
        {
            try
            {
                _userDataStore.Ping(user.ConnectionId);

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static string ProtectPayload(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                using (var writer = new StreamWriter(gzip))
                {
                    writer.Write(json);
                }

                return Convert.ToBase64String(output.ToArray());
            }
        }


        //یک نمونه هندلرد
        //(existingModule, incomingData) =>
        //            {
        //                var existing = existingModule;
        //                existing.Merge(incomingData, new JsonMergeSettings
        //                {
        //                    MergeArrayHandling = MergeArrayHandling.Replace,
        //                    MergeNullValueHandling = MergeNullValueHandling.Merge
        //                });
        //                return incomingData;
        //            },
    }
}