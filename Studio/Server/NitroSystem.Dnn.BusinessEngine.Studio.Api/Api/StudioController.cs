using System;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.AppModel;

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
        //private readonly IDatabaseMetadataRepository _databaseMetadata;

        public StudioController(
            IBaseService globalService,
            IEntityService entityService,
            IAppModelService appModelService,
            IServiceFactory serviceFactory,
            IDefinedListService definedListService
        //IDatabaseMetadataRepository databaseMetadata
        )
        {
            _globalService = globalService;
            _entityService = entityService;
            _appModelServices = appModelService;
            _serviceFactory = serviceFactory;
            _definedListService = definedListService;
            //_databaseMetadata = databaseMetadata;
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
                        result = await _appModelServices.UpdateGroupColumnAsync(item.ItemId, item.GroupId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteScenario(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _globalService.DeleteScenarioAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
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
        public async Task<HttpResponseMessage> DeleteGroup(GuidInfo postData)
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
                //var tables = _databaseMetadata.GetDatabaseObjectsAsync(0);
                //var views = _databaseMetadata.GetDatabaseObjectsAsync(1);

                return Request.CreateResponse(HttpStatusCode.OK/*, new { Tables = tables, Views = views }*/);
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
                //var columns = _databaseMetadata.GetDatabaseObjectColumnsAsync(objectName);

                return Request.CreateResponse(HttpStatusCode.OK/*, columns*/);
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
        public async Task<HttpResponseMessage> DeleteEntity(GuidInfo postData)
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
                var propertyTypes = Constants.VariableTypes.Select(kvp => new { Text = kvp.Key, Value = kvp.Value });

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
                appModel.Id = await _appModelServices.SaveAppModelAsync(appModel, appModel.Id == Guid.Empty, PortalSettings.HomeSystemDirectory);

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
        public async Task<HttpResponseMessage> DeleteAppModel(GuidInfo postData)
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
        public async Task<HttpResponseMessage> DeleteService(GuidInfo postData)
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
        public async Task<HttpResponseMessage> GetDefinedListByListName(string listName)
        {
            try
            {
                var definedList = await _definedListService.GetDefinedListByListName(listName);

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
    }
}