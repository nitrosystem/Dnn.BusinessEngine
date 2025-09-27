using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using DotNetNuke.Entities.Host;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;
using DotNetNuke.Entities.Controllers;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using System.Threading;
using System.Web.WebSockets;
using System.Net.WebSockets;
using System.Text;
using System.Data;
using System.IdentityModel.Metadata;
using System.Text.RegularExpressions;
using DotNetNuke.Collections;
using NitroSystem.Dnn.BusinessEngine.Shared.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using NitroSystem.Dnn.BusinessEngine.Shared.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class StudioController : DnnApiController
    {
        private readonly IBaseService _globalService;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelServices;
        private readonly IServiceFactory _serviceFactory;
        private readonly IDefinedListService _definedListService;
        private readonly IExtensionService _extensionService;

        public StudioController(
            IBaseService globalService,
            IEntityService entityService,
            IAppModelService appModelService,
            IServiceFactory serviceFactory,
            IExtensionService extensionService,
            IDefinedListService definedListService
        )
        {
            _globalService = globalService;
            _entityService = entityService;
            _appModelServices = appModelService;
            _serviceFactory = serviceFactory;
            _extensionService = extensionService;
            _definedListService = definedListService;
        }

        #region Common

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCacheAndAddCmsVersion()
        {
            try
            {
                HostController.Instance.Update("CrmVersion", (Host.CrmVersion + 1).ToString());

                DataCache.ClearCache();

                DataCache.ClearPortalCache(PortalSettings.PortalId, true);

                DataCache.ClearHostCache(true);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Sidebar Explorer

        [HttpGet]
        public async Task<HttpResponseMessage> GetStudioOptions()
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var scenarios = await _globalService.GetScenariosViewModelAsync();
                var scenario = await _globalService.GetScenarioViewModelAsync(scenarioId);
                var roles = await _globalService.GetPortalRolesAsync(PortalSettings.PortalId);
                var groups = await _globalService.GetGroupsViewModelAsync(scenarioId, "SidebarExplorer");
                var explorerItems = await _globalService.GetExplorerItemsViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Scenarios = scenarios,
                    Scenario = scenario,
                    Roles = roles,
                    Groups = groups,
                    ExplorerItems = explorerItems
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> RefreshSidebarExplorerItems()
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var explorerItems = await _globalService.GetExplorerItemsViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, explorerItems);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateItemGroup(ExplorerItemDto item)
        {
            try
            {
                bool result = false;

                switch (item.ItemType)
                {
                    case "Entity":
                        result = await _entityService.UpdateGroupColumn(item.ItemId, item.GroupId);
                        break;
                    case "ViewModel":
                        result = await _appModelServices.UpdateGroupColumn(item.ItemId, item.GroupId);
                        break;
                    case "Service":
                        result = await _serviceFactory.UpdateGroupColumn(item.ItemId, item.GroupId);
                        break;
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Scenario

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenarios()
        {
            try
            {
                var scenarios = await _globalService.GetScenariosViewModelAsync();

                return Request.CreateResponse(HttpStatusCode.OK, scenarios);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenario()
        {
            try
            {
                return await GetScenario(Guid.Empty);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenario(Guid scenarioId)
        {
            try
            {
                var scenario = await _globalService.GetScenarioViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveScenario(ScenarioViewModel scenario)
        {
            try
            {
                scenario.Id = await _globalService.SaveScenarioAsync(scenario, scenario.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Group

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveGroup(GroupViewModel group)
        {
            try
            {
                group.ScenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                group.Id = await _globalService.SaveGroupAsync(group, group.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, group.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteGroup(GuidDto postData)
        {
            try
            {
                var isDeleted = await _globalService.DeleteGroupAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Entities

        [DnnAuthorize]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEntities(int pageIndex, int pageSize, string searchText = null,
            byte? entityType = null, bool isReadonly = false, string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var entitiesTask = await _entityService.GetEntitiesViewModelAsync(scenarioId, pageIndex, pageSize,
                    searchText, entityType, isReadonly, sortBy);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Entities = entitiesTask.Items,
                    Page = new PagingInfo(entitiesTask.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntity()
        {
            return await GetEntity(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntity(Guid entityId)
        {
            try
            {
                var entity = await _entityService.GetEntityViewModelAsync(entityId);

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDatabaseObjects()
        {
            try
            {
                var tables = DbUtil.GetDatabaseObjects(0);
                var views = DbUtil.GetDatabaseObjects(1);

                return Request.CreateResponse(HttpStatusCode.OK, new { Tables = tables, Views = views });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDatabaseObjectColumns(string objectName)
        {
            try
            {
                var columns = DbUtil.GetDatabaseObjectColumns(objectName);

                return Request.CreateResponse(HttpStatusCode.OK, columns);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveEntity(EntityViewModel entity)
        {
            try
            {
                if (!entity.IsReadonly)
                    entity.TableName = (entity.Settings["DatabaseObjectPrefixName"].ToString() +
                                        entity.Settings["DatabaseObjectPostfixName"].ToString());

                entity.Id = await _entityService.SaveEntity(entity, entity.Id == Guid.Empty, HttpContext.Current);

                entity = await _entityService.GetEntityViewModelAsync(entity.Id);

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteEntity(GuidDto postData)
        {
            try
            {
                var isDeleted = await _entityService.DeleteEntityAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region App Models

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModels(int pageIndex, int pageSize, string searchText = null,
            string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var appModels = await _appModelServices.GetAppModelsAsync(scenarioId, pageIndex, pageSize, searchText, sortBy);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AppModels = appModels.Items,
                    Page = new PagingInfo(appModels.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModel()
        {
            return await GetAppModel(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModel(Guid appModelId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var appModels = await _appModelServices.GetAppModelsAsync(scenarioId, 1, 1000, null, "Title");
                var appModel = await _appModelServices.GetAppModelAsync(appModelId);
                var propertyTypes = GlobalItems.VariableTypes.Select(kvp => new { Text = kvp.Key, Value = kvp.Value });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AppModels = appModels,
                    AppModel = appModel,
                    PropertyTypes = propertyTypes,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveAppModel(AppModelViewModel appModel)
        {
            try
            {
                appModel.Id = await _appModelServices.SaveAppModelAsync(appModel, appModel.Id == Guid.Empty, PortalSettings);

                appModel = await _appModelServices.GetAppModelAsync(appModel.Id);

                return Request.CreateResponse(HttpStatusCode.OK, appModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteAppModel(GuidDto postData)
        {
            try
            {
                var isDeleted = await _appModelServices.DeleteAppModelAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Services

        [HttpGet]
        public async Task<HttpResponseMessage> GetServiceTypesListItem()
        {
            var serviceTypes = await _serviceFactory.GetServiceTypesListItemAsync();

            return Request.CreateResponse(HttpStatusCode.OK, serviceTypes);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetServices(int pageIndex, int pageSize, string searchText = null,
            string serviceType = null, string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var items = await _serviceFactory.GetServicesViewModelAsync(scenarioId, pageIndex, pageSize, searchText,
                    serviceType, sortBy);
                var services = items.Items;
                var totalCount = items.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Services = services,
                    Page = new PagingInfo(totalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetService(string serviceType)
        {
            return await GetService(serviceType, Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetService(string serviceType, Guid serviceId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var items = await _serviceFactory.GetServiceViewModelAsync(scenarioId, serviceType, serviceId);
                var service = items.Service;
                var extension = items.Extension;
                var extensionDependency = items.ExtensionDependency;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Service = service,
                    ExtensionService = extension,
                    ExtensionDependency = extensionDependency
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetServiceParams(Guid serviceId)
        {
            try
            {
                var serviceParams = await _serviceFactory.GetServiceParamsAsync(serviceId);

                return Request.CreateResponse(HttpStatusCode.OK, serviceParams);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveService(SaveServiceDto postData)
        {
            try
            {
                var result = await _serviceFactory.SaveServiceAsync(postData.Service, postData.ExtensionServiceJson, postData.Service.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ServiceId = result.ServiceId,
                    ExtensionServiceId = result.ExtensionServiceId
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteService(GuidDto postData)
        {
            try
            {
                var isDeleted = await _serviceFactory.DeleteServiceAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Defined Lists

        [HttpGet]
        public async Task<HttpResponseMessage> GetDefinedListByFieldId(Guid fieldId)
        {
            try
            {
                var definedList = await _definedListService.GetDefinedListByFieldId(fieldId);

                return Request.CreateResponse(HttpStatusCode.OK, definedList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDefinedList(DefinedListViewModel definedList)
        {
            try
            {
                definedList.Id = await _definedListService.SaveDefinedList(definedList, definedList.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, definedList.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Extensions

        [HttpGet]
        public async Task<HttpResponseMessage> GetExtensions()
        {
            try
            {
                var extensions = await _extensionService.GetExtensionsViewModelAsync();

                return Request.CreateResponse(HttpStatusCode.OK, extensions);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<HttpResponseMessage> InstallExtension()
        //{
        //    try
        //    {
        //        if (!UserInfo.IsSuperUser)
        //            return Request.CreateResponse(HttpStatusCode.Forbidden, "Only superusers can install extensions.");

        //        if (!Request.Content.IsMimeMultipartContent())
        //            return Request.CreateResponse(HttpStatusCode.InternalServerError, BadRequest("Invalid request format. Multipart content expected."));

        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        // Create temp upload folder
        //        var uploadPath = Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, @"business-engine\temp\");
        //        Directory.CreateDirectory(uploadPath);

        //        var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(uploadPath);
        //        await Request.Content.ReadAsMultipartAsync(streamProvider);

        //        var filename = uploadPath + Path.GetFileName(streamProvider.FileData[0].LocalFileName);
        //        var fileExt = Path.GetExtension(filename);

        //        // Extension whitelist check
        //        if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(fileExt))
        //            return Request.CreateResponse(HttpStatusCode.InternalServerError, BadRequest($"File type '{fileExt}' is not allowed."));

        //        var result = await _extensionService.InstallExtensionAsync(scenarioId, filename);

        //        return Request.CreateResponse(HttpStatusCode.OK, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage InstallExtension([FromUri] Guid installTemporaryItemId)
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        string monitoringFile = string.Empty;
        //        string progressFile = string.Empty;
        //        ProgressMonitoring monitoringInstance = null;

        //        monitoringFile = Request.Headers.GetValues("MonitoringFile").First();
        //        progressFile = Request.Headers.GetValues("ProgressFile").First();

        //        monitoringInstance = new ProgressMonitoring(monitoringFile, progressFile, "Start instaling the extension...");

        //        await ws.SendMessageToClientAsync("Loading extension install temporary...", 5);
        //        Thread.Sleep(500);
        //        var objExtensionInstallTemporaryInfo = ExtensionInstallTemporaryRepository.Instance.GetExpression(installTemporaryItemId);

        //        await ws.SendMessageToClientAsync("Deserializing json to modeling object...", 10);
        //        Thread.Sleep(500);
        //        var extension = JsonConvert.DeserializeObject<ExtensionManifest>(objExtensionInstallTemporaryInfo.ExtensionManifestJson);

        //        var extensionController = new ExtensionService(scenarioId, PortalSettings, this.UserInfo);
        //        extensionController.InstallExtension(extension, objExtensionInstallTemporaryInfo.ExtensionInstallUnzipedPath, monitoringInstance);

        //        ExtensionInstallTemporaryRepository.Instance.DeleteItem(installTemporaryItemId);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteExtension(GuidDto postData)
        {
            try
            {
                var result = await _extensionService.UninstallExtension(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Providers

        [HttpGet]
        public async Task<HttpResponseMessage> GetProvider(Guid providerId)
        {
            try
            {
                //var provider = ProviderRepository.Instance.GetProvider(providerId);

                return Request.CreateResponse(HttpStatusCode.OK/*, provider*/);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveProvider(object provider)
        {
            try
            {
                //ProviderRepository.Instance.UpdateProvider(provider);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion
    }
}