using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Controllers;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Core.WebApi;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class StudioController : DnnApiController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheService _cacheService;
        private readonly IBaseService _baseService;
        private readonly IEntityService _entityService;
        private readonly IAppModelService _appModelServices;
        private readonly IServiceFactory _serviceFactory;
        private readonly IDefinedListService _definedListService;
        private readonly IExtensionService _extensionService;
        private readonly string _basePath;

        public StudioController(
            IServiceProvider serviceProvider,
            ICacheService cacheService,
            IBaseService baseService,
            IEntityService entityService,
            IAppModelService appModelService,
            IServiceFactory serviceFactory,
            IDefinedListService definedListService,
            IExtensionService extensionService
            )
        {
            _serviceProvider = serviceProvider;
            _cacheService = cacheService;
            _baseService = baseService;
            _entityService = entityService;
            _appModelServices = appModelService;
            _serviceFactory = serviceFactory;
            _definedListService = definedListService;
            _extensionService = extensionService;
            _basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
        }

        #region Common

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCacheAndAddCmsVersion()
        {
            try
            {
                HostController.Instance.Update("CrmVersion", (Host.CrmVersion + 1).ToString());
                _cacheService.ClearAll();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Sidebar Explorer

        [HttpGet]
        public async Task<HttpResponseMessage> GetStudioOptions(Guid? scenarioId = null)
        {
            try
            {
                var scenarios = await _baseService.GetScenariosViewModelAsync();

                if (!scenarioId.HasValue) return Request.CreateResponse(HttpStatusCode.OK, new { Scenarios = scenarios });

                var scenario = await _baseService.GetScenarioViewModelAsync(scenarioId.Value);
                var roles = GetPortalRolesAsync();
                var groups = await _baseService.GetGroupsViewModelAsync(scenarioId.Value, "SidebarExplorer");
                var explorerItems = await _baseService.GetExplorerItemsViewModelAsync(scenarioId.Value);

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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> RefreshSidebarExplorerItems(Guid scenarioId)
        {
            try
            {
                var explorerItems = await _baseService.GetExplorerItemsViewModelAsync(scenarioId);
                var groups = await _baseService.GetGroupsViewModelAsync(scenarioId, "SidebarExplorer");

                return Request.CreateResponse(HttpStatusCode.OK, new { ExplorerItems = explorerItems, Groups = groups });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateItemGroup(GroupItemDto item)
        {
            try
            {
                switch (item.GroupType)
                {
                    case "Entity":
                        await _entityService.UpdateEntityGroupAsync(item.GroupId, item.ItemId);
                        break;
                    case "AppModel":
                        await _appModelServices.UpdateAppMopdelGroupAsync(item.GroupId, item.ItemId);
                        break;
                    case "Service":
                        await _serviceFactory.UpdateGroupColumnAsync(item.GroupId, item.ItemId);
                        break;
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Scenario

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenarios()
        {
            try
            {
                var scenarios = await _baseService.GetScenariosViewModelAsync();
                return Request.CreateResponse(HttpStatusCode.OK, scenarios);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenario(Guid? scenarioId = null)
        {
            try
            {
                var scenario = scenarioId.HasValue
                    ? await _baseService.GetScenarioViewModelAsync(scenarioId.Value)
                    : null;
                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveScenario(ScenarioViewModel scenario)
        {
            try
            {
                scenario.Id = await _baseService.CreateScenarioAsync(scenario);
                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteScenario(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _baseService.DeleteScenarioAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                group.Id = await _baseService.CreateGroupAsync(group);
                return Request.CreateResponse(HttpStatusCode.OK, group.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteGroup(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _baseService.DeleteGroupAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Entities

        [DnnAuthorize]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEntities(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText = null,
            byte? entityType = null,
            bool? isReadonly = null,
            string sortBy = "ViewOrder")
        {
            try
            {
                var results = await _entityService.GetEntitiesViewModelAsync(scenarioId, pageIndex, pageSize, searchText, entityType, isReadonly, sortBy);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Entities = results.Items,
                    Page = new PagingInfo(results.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntity(Guid? entityId = null)
        {
            try
            {
                var entity = entityId.HasValue
                    ? await _entityService.GetEntityViewModelAsync(entityId.Value)
                    : null;
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDatabaseObjects()
        {
            try
            {
                var results = await _baseService.GetDatabaseObjectsAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { results.Tables, results.Views });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDatabaseObjectColumns(string objectName)
        {
            try
            {
                var columns = await _baseService.GetDatabaseObjectColumnsAsync(objectName);
                return Request.CreateResponse(HttpStatusCode.OK, columns);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveEntity(EntityViewModel entity)
        {
            try
            {
                if (!entity.IsReadonly)
                {
                    if (!entity.Settings.TryGetValue("DatabaseObjectPrefixName", out var prefix))
                        throw new Exception("Invalid PrefixName");
                    if (!entity.Settings.TryGetValue("DatabaseObjectPostfixName", out var postfix))
                        throw new Exception("Invalid PrefixName");

                    entity.TableName = prefix.ToString() + postfix.ToString();
                }

                entity.Id = await _entityService.CreateEntityAsync(entity);
                entity = await _entityService.GetEntityViewModelAsync(entity.Id);

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteEntity(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _entityService.DeleteEntityAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region App Models

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModels(Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText = null,
            AppModelType? modelType = null,
            string sortBy = "ViewOrder")
        {
            try
            {
                var results = await _appModelServices.GetAppModelsViewModelAsync(scenarioId, pageIndex, pageSize, searchText, modelType, sortBy);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AppModels = results.Items,
                    Page = new PagingInfo(results.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAppModel(Guid scenarioId, Guid? appModelId = null)
        {
            try
            {
                var entities = await _entityService.GetEntitiesListItemAsync(scenarioId, "Title");
                var appModel = appModelId.HasValue
                    ? await _appModelServices.GetAppModelViewModelAsync(appModelId.Value)
                    : null;
                var propertyTypes = Constants.VariableTypes
                    .Where(vt => vt.Key != "AppModel" && vt.Key != "AppModelList")
                    .Select(vt => new { Text = vt.Key, Value = vt.Value });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Entities = entities,
                    AppModel = appModel,
                    PropertyTypes = propertyTypes,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveAppModel(AppModelViewModel appModel)
        {
            try
            {
                string relativePath = PortalSettings.HomeSystemDirectory + @"business-engine/";
                appModel.Id = await _appModelServices.CreateAppModelAsync(appModel, relativePath, _basePath);

                return Request.CreateResponse(HttpStatusCode.OK, appModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteAppModel(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _appModelServices.DeleteAppModelAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Services

        [HttpGet]
        public async Task<HttpResponseMessage> GetServiceTypesListItem()
        {
            var serviceTypes = await _serviceFactory.GetServiceTypesListItemAsync("ServiceDomain");
            return Request.CreateResponse(HttpStatusCode.OK, serviceTypes);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetServices(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText = null,
            string serviceDomain = null,
            string serviceType = null,
            string sortBy = "ViewOrder")
        {
            try
            {
                var serviceTypes = await _serviceFactory.GetServiceTypesListItemAsync("ServiceDomain", "ServiceType");
                var results = await _serviceFactory.GetServicesViewModelAsync(scenarioId, pageIndex, pageSize,
                    searchText, serviceDomain, serviceType, sortBy);
                var services = results.Items;
                var totalCount = results.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ServiceTypes = serviceTypes,
                    Services = services,
                    Page = new PagingInfo(totalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetService(Guid scenarioId, string serviceType = "", Guid? serviceId = null)
        {
            try
            {
                var results = await _serviceFactory.GetServiceViewModelAsync(scenarioId, serviceType, serviceId);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Service = results.Service,
                    ExtensionService = results.Extension,
                    ExtensionDependency = results.ExtensionDependency,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveService(SaveServiceDto postData)
        {
            try
            {
                var results = await _serviceFactory.CreateServiceAsync(postData.Service, postData.ExtensionServiceJson);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ServiceId = results.ServiceId,
                    ExtensionServiceId = results.ExtensionServiceId
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteService(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _serviceFactory.DeleteServiceAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Defined Lists

        [HttpGet]
        public async Task<HttpResponseMessage> GetDefinedLists(Guid scenarioId)
        {
            try
            {
                var definedLists = await _definedListService.GetDefinedListsViewModelAsync(scenarioId);
                return Request.CreateResponse(HttpStatusCode.OK, definedLists);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDefinedListByListName(string listName = "")
        {
            try
            {
                var definedList = !string.IsNullOrEmpty(listName)
                    ? await _definedListService.GetDefinedListViewModelByListNameAsync(listName)
                    : null;
                return Request.CreateResponse(HttpStatusCode.OK, definedList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDefinedList(DefinedListViewModel definedList)
        {
            try
            {
                definedList.Id = await _definedListService.CreateDefinedListAsync(definedList);
                return Request.CreateResponse(HttpStatusCode.OK, definedList.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                var availableExtensions = _extensionService.GetAvailableExtensionsViewModel();

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { Extensions = extensions, AvailableExtensions = availableExtensions });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetAvailableExtension(string extensionFilename)
        {
            try
            {
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                var results = _extensionService.GetAvailableExtension(extensionFilename, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    results.Manifest,
                    results.ExtractPath
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadExtensionPackage()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, BadRequest("Invalid request format. Multipart content expected."));

                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                string uploadPath = _extensionService.GetOrCreateExtensionTempPath(basePath);

                var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(uploadPath);
                await Request.Content.ReadAsMultipartAsync(streamProvider);
                var results = _extensionService.GetExtensionManifest(streamProvider.FileData[0].LocalFileName, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    results.Manifest,
                    results.ExtractPath
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> InstallExtension(InstallExtensionRequest request)
        {
            await _extensionService.InstallExtension(request);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Private Methods

        private string[] GetPortalRolesAsync()
        {
            string cacheKey = "bPortalRoles";
            string[] result = _cacheService.Get<string[]>(cacheKey);

            if (result == null)
            {
                var roles = RoleController.Instance.GetRoles(PortalSettings.PortalId)
                    .Cast<RoleInfo>()
                    .Select(r => r.RoleName)
                    .ToArray();

                var allUsers = new string[] { "All Users" };
                result = allUsers.Concat(roles).ToArray();

                _cacheService.Set(cacheKey, result);
            }

            return result;
        }

        #endregion
    }
}