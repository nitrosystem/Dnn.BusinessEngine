using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.App.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.App.DataServices.ModuleData;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

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

                await _actionWorker.CallActions(moduleData, moduleId, null, "OnPageLoad");

                var data = _userDataStore.GetDataForClients(moduleId, moduleData);

                var variables = await _moduleService.GetModuleVariables(moduleId, ModuleVariableScope.Global);

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

                await _actionWorker.CallActions(action.ActionIds, moduleData);

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
    }
}