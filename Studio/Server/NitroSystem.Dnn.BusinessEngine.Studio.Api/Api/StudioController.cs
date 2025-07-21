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
using NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO;
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
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Utilities;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class StudioController : DnnApiController
    {
        private readonly IGlobalService _globalService;
        private readonly IEntityService _entityService;
        private readonly IViewModelService _viewModelService;
        private readonly IServiceFactory _serviceFactory;
        private readonly IDefinedListService _definedListService;

        public StudioController(
            IGlobalService globalService,
            IEntityService entityService,
            IViewModelService viewModelService,
            IServiceFactory serviceFactory,
            IDefinedListService definedListService
        )
        {
            _globalService = globalService;
            _entityService = entityService;
            _viewModelService = viewModelService;
            _serviceFactory = serviceFactory;
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
        public async Task<HttpResponseMessage> UpdateItemGroup(ExplorerItemDTO item)
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
                        result = await _viewModelService.UpdateGroupColumn(item.ItemId, item.GroupId);
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
        public async Task<HttpResponseMessage> DeleteGroup(GuidDTO postData)
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
        public async Task<HttpResponseMessage> DeleteEntity(GuidDTO postData)
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

        #region View Models

        [HttpGet]
        public async Task<HttpResponseMessage> GetViewModels(int pageIndex, int pageSize, string searchText = null,
            string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var viewModels =
                    await _viewModelService.GetViewModelsAsync(scenarioId, pageIndex, pageSize, searchText, sortBy);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ViewModels = viewModels.Items,
                    Page = new PagingInfo(viewModels.TotalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetViewModel()
        {
            return await GetViewModel(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetViewModel(Guid viewModelId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var viewModels = await _viewModelService.GetViewModelsAsync(scenarioId, 1, 1000, null, "Title");
                var viewModel = await _viewModelService.GetViewModelAsync(viewModelId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ViewModels = viewModels,
                    ViewModel = viewModel,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveViewModel(ViewModelViewModel viewModel)
        {
            try
            {
                viewModel.Id = await _viewModelService.SaveViewModelAsync(viewModel, viewModel.Id == Guid.Empty);

                viewModel = await _viewModelService.GetViewModelAsync(viewModel.Id);

                return Request.CreateResponse(HttpStatusCode.OK, viewModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteViewModel(GuidDTO postData)
        {
            try
            {
                var isDeleted = await _viewModelService.DeleteViewModelAsync(postData.Id);

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
            var serviceTypes = await _serviceFactory.GetServiceTypesDtoAsync();

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
        public async Task<HttpResponseMessage> DeleteService(GuidDTO postData)
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