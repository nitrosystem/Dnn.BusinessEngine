using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Concurrent;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;
using NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate;
using Microsoft.Extensions.DependencyInjection;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.Nitro_I.Fields.Api
{
    public class ServiceController : DnnApiController
    {
        private readonly IModuleFieldService _moduleFieldService;
        private readonly IActionService _actionService;
        private readonly IUserDataStore _userDataStore;
        private readonly ActionRunner _actionRunner;

        public ServiceController(IServiceProvider serviceProvider)
        {
            _moduleFieldService = serviceProvider.GetRequiredService<IModuleFieldService>();
            _actionService = serviceProvider.GetRequiredService<IActionService>();
            _userDataStore = serviceProvider.GetRequiredService<IUserDataStore>();
            _actionRunner = serviceProvider.GetRequiredService<ActionRunner>();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<HttpResponseMessage> GetListTemplate(string connectionId, Guid moduleId, Guid fieldId, Guid actionId, string pageUrl)
        {
            try
            {
                var basePath = PortalSettings.HomeSystemDirectoryMapPath;
                var field = await _moduleFieldService.GetModuleFieldDtoAsync(fieldId, true);
                var template = field.Settings["Content"]?.ToString();
                template = template.Replace("[FIELD]", $"field.{field.FieldName}");
                template = template.Replace("[FIELDNAME]", $"{field.FieldName}");
                template = template.Replace("[FIELDID]", $"{field.Id}");

                var moduleData = await _userDataStore.GetOrCreateModuleDataAsync(connectionId, moduleId, basePath);
                var actions = await _actionService.GetActionsAsync(moduleId, fieldId, actionId);
                if (actions.Count > 0)
                    await _actionRunner.ExecuteAsync(
                        actions,
                        connectionId,
                        moduleId,
                        PortalSettings.UserId,
                        pageUrl,
                        basePath,
                        moduleData);

                var data = _userDataStore.GetDataForClients(connectionId, moduleId) ?? new ConcurrentDictionary<string, object>();

                var context = new RenderContext(data);
                var builder = new BuildTemplate();
                var parser = new TemplateParser();
                var parsedTemplate = parser.Parse(template);
                template = builder.Render(parsedTemplate, context);

                return Request.CreateResponse(HttpStatusCode.OK, template);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #region Import Export Api

        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<HttpResponseMessage> ImportExcelApi()
        //{
        //    string filename = string.Empty;
        //    var currentRequest = HttpContext.Current.Request;

        //    try
        //    {
        //        if (HttpContext.Current.Request.Files.Count > 0)
        //        {
        //            var fileName = HttpContext.Current.Request.Files[0].FileName;
        //            if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
        //                throw new Exception("File type not allowed");
        //        }

        //        if (Request.Content.IsMimeMultipartContent())
        //        {
        //            string uploadPath = PortalSettings.HomeDirectory + "BusinessEngine/ImportedExcelFiles/";
        //            var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
        //            if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

        //            var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

        //            await Request.Content.ReadAsMultipartAsync(streamProvider);

        //            foreach (var file in streamProvider.FileData)
        //            {
        //                filename = HttpContext.Current.Server.MapPath(uploadPath + Path.GetFileName(file.LocalFileName));
        //            }
        //        }
        //        else
        //        {
        //            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request!");
        //            throw new HttpResponseException(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }

        //    try
        //    {
        //        var model = new ImportExcelInfo()
        //        {
        //            Filename = filename,
        //            //FromRow = int.Parse(currentRequest.Params["FromRow"]),
        //            //ToRow = int.Parse(currentRequest.Params["ToRow"]),
        //            //FromColumn = int.Parse(currentRequest.Params["FromColumn"]),
        //            //ToColumn = int.Parse(currentRequest.Params["ToColumn"]),
        //            HeaderColumns = JsonConvert.DeserializeObject<List<ExcelColumnInfo>>(currentRequest.Params["Columns"]).OrderBy(c => c.ViewOrder).ToList()
        //        };

        //        var serviceResult = ImportExcel.Import(model);

        //        return Request.CreateResponse(HttpStatusCode.OK, serviceResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion
    }
}
