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
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.WebSocketServer;
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

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class StudioController : DnnApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IGlobalService _globalService;
        private readonly IDefinedListService _definedListService;

        public StudioController(
            ICacheService cacheService,
            IUnitOfWork unitOfWork,
            IDbConnection connection,
            IGlobalService globalService,
            IDefinedListService definedListService
        )
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _globalService = globalService;
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

        //[HttpGet]
        //public HttpResponseMessage GetSettings(string groupName)
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        var settings = GlobalSettingsRepository.Instance.GetSettings(scenarioId, groupName);

        //        return Request.CreateResponse(HttpStatusCode.OK, settings);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SaveSettings([FromUri] string groupName, Idictionary<string, object> settings)
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        foreach (var item in settings)
        //        {
        //            GlobalSettingsRepository.Instance.UpdateSetting(scenarioId, groupName, item.Key, item.Value);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion

        #region Sidebar Explorer

        [HttpGet]
        public async Task<HttpResponseMessage> GetStudioOptions()
        {
            try
            {
                var globalService = new GlobalService(_unitOfWork, _cacheService);

                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var scenariosTask = globalService.GetScenariosViewModelAsync();
                var scenarioTask = globalService.GetScenarioViewModelAsync(scenarioId);
                var rolesTask = globalService.GetPortalRolesAsync(PortalSettings.PortalId);
                var groupsTask = globalService.GetGroupsViewModelAsync(scenarioId, "SidebarExplorer");
                var explorerItemsTask = globalService.GetExplorerItemsViewModelAsync(scenarioId);

                await Task.WhenAll(scenariosTask, scenarioTask, rolesTask, groupsTask, explorerItemsTask);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Scenarios = scenariosTask.Result,
                    Scenario = scenarioTask.Result,
                    Roles = rolesTask.Result,
                    Groups = groupsTask.Result,
                    ExplorerItems = explorerItemsTask.Result
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage RefreshSidebarExplorerItems()
        {
            try
            {
                var globalService = new GlobalService(_unitOfWork, _cacheService);

                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var explorerItems = globalService.GetExplorerItemsViewModelAsync(scenarioId);

                return Request.CreateResponse(HttpStatusCode.OK, explorerItems);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage UpdateItemGroup(ExplorerItemDTO item)
        //{
        //    try
        //    {
        //        var service = new GlobalService(_unitOfWork,_cacheService);

        //        switch (item.ItemType)
        //        {
        //            case "Entity":
        //                service.UpdateColumnAsync<EntityViewModel>(new EntityInfo)
        //                EntityRepository.Instance.UpdateGroup(item.ItemId, item.GroupId);
        //                break;
        //            case "ViewModel":
        //                ViewModelRepository.Instance.UpdateGroup(item.ItemId, item.GroupId);
        //                break;
        //            case "Service":
        //                ServiceRepository.Instance.UpdateGroup(item.ItemId, item.GroupId);
        //                break;
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion

        #region Scenario

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenarios()
        {
            try
            {
                var globalService = new GlobalService(_unitOfWork, _cacheService);
                var scenarios = await globalService.GetScenariosViewModelAsync();

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
                _unitOfWork.Rollback();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetScenario(Guid scenarioId)
        {
            try
            {
                var globalService = new GlobalService(_unitOfWork, _cacheService);
                var scenario = await globalService.GetScenarioViewModelAsync(scenarioId);

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
                var globalService = new GlobalService(_unitOfWork, _cacheService);
                scenario.Id = await globalService.SaveScenarioAsync(scenario, scenario.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, scenario);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeleteScenarioAndChilds(GuidDTO postData)
        //{
        //    try
        //    {
        //        var controller = new GlobalService(_unitOfWork,_cacheService);
        //        controller.DeleteScenarioAndChilds(postData.Id);
        //        _unitOfWork.Commit();

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        _unitOfWork.Rollback();
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpGet]
        //public HttpResponseMessage GetScenarioModulesAndFields(Guid scenarioId)
        //{
        //    try
        //    {
        //        var modules = ModuleRepository.Instance.GetScenarioModules(scenarioId);

        //        var result = new List<object>();

        //        foreach (var module in modules.Where(m => m.ModuleBuilderType != "HtmlEditor"))
        //        {
        //            result.Add(new
        //            {
        //                ModuleId = module.Id,
        //                ParentId = module.ParentId,
        //                ModuleType = module.ModuleType,
        //                LayoutTemplate = module.LayoutTemplate,
        //                LayoutCss = module.LayoutCss,
        //                Fields = ModuleFieldMappings.GetFieldsViewModel(module.Id)
        //            });
        //        }

        //        var fieldTypes = ModuleFieldMappings.GetFieldTypesViewModel();

        //        PageResourceRepository.Instance.DeleteScenarioPageResources(scenarioId);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { Modules = result, FieldTypes = fieldTypes });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        #endregion

        #region Group

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveGroup(GroupViewModel group)
        {
            try
            {
                group.ScenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var globalService = new GlobalService(_unitOfWork, _cacheService);
                group.Id = await globalService.SaveGroupAsync(group, group.Id == Guid.Empty);

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
                var globalService = new GlobalService(_unitOfWork, _cacheService);
                var isDeleted = await globalService.DeleteGroupAsync(postData.Id);

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

                var entityService = new EntityService(_unitOfWork, _cacheService);
                var entitiesTask = await entityService.GetEntitiesViewModelAsync(scenarioId, pageIndex, pageSize,
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

        //[HttpGet]
        //public HttpResponseMessage GetDatabaseObjects(Guid? databaseId = null)
        //{
        //    try
        //    {
        //        string connectionString = "";

        //        if (databaseId != null)
        //        {
        //            var database = Data.Repositories.DatabaseRepository.Instance.GetDatabase(databaseId.Value);
        //            connectionString = database != null ? database.ConnectionString : "";
        //        }

        //        var tables = DbUtil.GetDatabaseObjects(0, connectionString);

        //        var views = DbUtil.GetDatabaseObjects(1, connectionString);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { Tables = tables, Views = views });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpGet]
        //public HttpResponseMessage GetDatabaseObjectColumns(string objectName, Guid? databaseId = null)
        //{
        //    try
        //    {
        //        string connectionString = "";

        //        if (databaseId != null)
        //        {
        //            var database = Data.Repositories.DatabaseRepository.Instance.GetDatabase(databaseId.Value);
        //            connectionString = database != null ? database.ConnectionString : "";
        //        }

        //        var columns = DbUtil.GetDatabaseObjectColumns(objectName, connectionString);

        //        return Request.CreateResponse(HttpStatusCode.OK, columns);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

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
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var globalService = new GlobalService(_unitOfWork, _cacheService);
                var entityService = new EntityService(_unitOfWork, _cacheService);

                var scenariosTask = globalService.GetScenariosViewModelAsync();
                var groupsTask = globalService.GetGroupsViewModelAsync(scenarioId, "Entity");
                var entityTask = entityService.GetEntityViewModelAsync(entityId);

                await Task.WhenAll(scenariosTask, groupsTask, entityTask);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Scenarios = scenariosTask.Result,
                    Groups = groupsTask.Result,
                    Entity = entityTask.Result,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEntitiesForRelationsheeps()
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var entityService = new EntityService(_unitOfWork, _cacheService);
                var entities =
                    await entityService.GetEntitiesViewModelAsync(scenarioId, 1, 1000, null, null, null, "Title");

                return Request.CreateResponse(HttpStatusCode.OK, entities);
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
                var ws = new WebSocketServer();
                await ws.ConnectAsync();
                await ws.SendMessageToClientAsync(10, "Starting to save Entity...");

                var service = new EntityService(_unitOfWork, _cacheService);
                
                _unitOfWork.BeginTransaction();

                if (!entity.IsReadonly)
                    entity.TableName = (entity.Settings["DatabaseObjectPrefixName"].ToString() +
                                        entity.Settings["DatabaseObjectPostfixName"].ToString());

                entity.Id = await service.SaveEntity(entity, entity.Id == Guid.Empty, HttpContext.Current, ws);
                
                _unitOfWork.Commit();

                entity = await service.GetEntityViewModelAsync(entity.Id);

                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteEntity(GuidDTO postData)
        {
            try
            {
                var service = new EntityService(_unitOfWork, _cacheService);
                _unitOfWork.BeginTransaction();

                var isDeleted = await service.DeleteEntityAsync(postData.Id);
                _unitOfWork.Commit();

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

                var viewModelService = new ViewModelService(_unitOfWork, _cacheService);
                var viewModels =
                    await viewModelService.GetViewModelsAsync(scenarioId, pageIndex, pageSize, searchText, sortBy);

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

        //[HttpGet]
        //public HttpResponseMessage GetViewModelsFromEntity(Guid entityId)
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        var scenarios = ScenarioRepository.Instance.GetScenarios();

        //        var groups = GroupRepository.Instance.GetGroups(scenarioId).Where(g => g.ObjectType == "ViewModel");

        //        var viewModels = ViewModelRepository.Instance.GetViewModels(scenarioId);

        //        var entity = EntityMapping.GetEntityViewModel(entityId);

        //        var viewModel = new ViewModelViewModel()
        //        {
        //            ScenarioId = entity.ScenarioId,
        //            ViewModelName = entity.EntityName + "ViewModel",
        //            ViewOrder = entity.ViewOrder,
        //            CreatedOnDate = DateTime.Now,
        //            CreatedByUserId = this.UserInfo.UserId,
        //            LastModifiedOnDate = DateTime.Now,
        //            LastModifiedByUserId = this.UserInfo.UserId,
        //            Description = entity.Description,
        //            Properties = entity.Columns.Select(c => new ViewModelPropertyInfo()
        //            {
        //                PropertyName = c.ColumnName,
        //                PropertyType = TypeProvider.ConvertSqlServerFormatToCSharp(c.ColumnType) ?? "nvarchar(max)",
        //                ViewOrder = c.ViewOrder
        //            })
        //        };

        //        return Request.CreateResponse(HttpStatusCode.OK, viewModel);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

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

                var globalService = new GlobalService(_unitOfWork, _cacheService);
                var viewModelService = new ViewModelService(_unitOfWork, _cacheService);

                var task1 = globalService.GetScenariosViewModelAsync();
                var task2 = globalService.GetGroupsViewModelAsync(scenarioId);
                var task3 = viewModelService.GetViewModelsAsync(scenarioId, 1, 1000, null, "Title");
                var task4 = viewModelService.GetViewModelAsync(viewModelId);

                await Task.WhenAll(task1, task2, task3, task4);

                var scenarios = task1.Result;
                var groups = task2.Result.Where(g => g.ObjectType == "ViewModel");
                var viewModels = task3.Result;
                var viewModel = task4.Result;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Scenarios = scenarios,
                    Groups = groups,
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
                var service = new ViewModelService(_unitOfWork, _cacheService);
                var id = await service.SaveViewModelAsync(viewModel, viewModel.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, id);
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
                _unitOfWork.BeginTransaction();

                var service = new ViewModelService(_unitOfWork, _cacheService);
                var isDeleted = await service.DeleteViewModelAsync(postData.Id);

                _unitOfWork.Commit();

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
        public async Task<HttpResponseMessage> GetServices(int pageIndex, int pageSize, string searchText = null,
            string serviceType = null, string serviceSubtype = null, string sortBy = "Newest")
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var serviceFactory = new ServiceFactory(_unitOfWork, _cacheService);

                var task1 = serviceFactory.GetServiceTypeViewModelsAsync();
                var task2 = serviceFactory.GetServicesViewModelAsync(scenarioId, pageIndex, pageSize, searchText,
                    serviceType, serviceSubtype, sortBy);

                await Task.WhenAll(task1, task2);

                var serviceTypes = task1.Result;
                var services = task2.Result.Items;
                var totalCount = task2.Result.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ServiceTypes = serviceTypes,
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
        public async Task<HttpResponseMessage> GetService()
        {
            return await GetService(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetService(Guid serviceId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var globalService = new GlobalService(_unitOfWork, _cacheService);
                var serviceFactory = new ServiceFactory(_unitOfWork, _cacheService);

                var task1 = globalService.GetScenariosViewModelAsync();
                var task2 = globalService.GetGroupsViewModelAsync(scenarioId);
                var task3 = serviceFactory.GetServiceViewModelAsync(serviceId);
                var task4 = serviceFactory.GetServiceTypeViewModelsAsync();

                await Task.WhenAll(task1, task2, task3, task4);

                var scenarios = task1.Result;
                var groups = task2.Result;
                var service = task3.Result;
                var serviceTypes = task4.Result;

                IEnumerable<string> roles = null;
                roles = RoleController.Instance.GetRoles(PortalSettings.PortalId).Cast<RoleInfo>()
                    .Select(r => r.RoleName);
                var allUsers = new List<string>();
                allUsers.Add("All Users");
                roles = allUsers.Concat(roles);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Service = service,
                    Scenarios = scenarios,
                    Groups = groups,
                    ServiceTypes = serviceTypes,
                    Roles = roles,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpGet]
        //public HttpResponseMessage GetServiceDependencies()
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        var entities = EntityMapping.GetEntitiesViewModel(scenarioId);

        //        var viewModels = ViewModelMapping.GetViewModelsViewModel(scenarioId);

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            Entities = entities,
        //            ViewModels = viewModels,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        [HttpGet]
        public async Task<HttpResponseMessage> GetServiceParams(Guid serviceId)
        {
            try
            {
                var serviceFactory = new ServiceFactory(_unitOfWork, _cacheService);
                var serviceParams = await serviceFactory.GetServiceParamsAsync(serviceId);

                return Request.CreateResponse(HttpStatusCode.OK, serviceParams);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveService(ServiceViewModel service)
        {
            try
            {
                var serviceFactory = new ServiceFactory(_unitOfWork, _cacheService);
                await serviceFactory.SaveServiceAsync(service, service.Id == Guid.Empty);
                _unitOfWork.Commit();

                bool isNew = service.Id == Guid.Empty;

                return Request.CreateResponse(HttpStatusCode.OK, service);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteService(GuidDTO postData)
        {
            try
            {
                var service = new ServiceFactory(_unitOfWork, _cacheService);
                var isDeleted = await service.DeleteServiceAsync(postData.Id);
                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpGet]
        //public HttpResponseMessage GetServiceSpScript(Guid serviceId)
        //{
        //    try
        //    {
        //        string result = string.Empty;

        //        var service = ServiceRepository.Instance.GetService(serviceId);

        //        var connection = new System.Data.SqlClient.SqlConnection(DotNetNuke.Data.DataProvider.Instance().ConnectionString);

        //        //service.DatabaseObjectParams = Dapper.SqlMapper.Query<string>(connection, string.Format("SELECT [Name] FROM sys.parameters WHERE object_id = object_id('dbo.{0}')", service.DatabaseObjectName));

        //        // result = Dapper.SqlMapper.QuerySingle<string>(connection, string.Format("SELECT [Definition] FROM sys.sql_modules WHERE objectproperty(OBJECT_Id, 'IsProcedure') = 1 AND OBJECT_NAME(OBJECT_Id) = '{0}'", service.DatabaseObjectName));
        //        // or use EXEC sp_helptext N'dbo.GetFolders'

        //        return Request.CreateResponse(HttpStatusCode.OK/*, result*/);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion

        //#endregion

        //#endregion

        #region Defined Lists

        //[HttpGet]
        //public HttpResponseMessage GetDefinedLists()
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        var result = DefinedListMapping.GetListsViewModel(scenarioId);

        //        return Request.CreateResponse(HttpStatusCode.OK, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

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
                _unitOfWork.BeginTransaction();

                definedList.Id = await _definedListService.SaveDefinedList(definedList, definedList.Id == Guid.Empty);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, definedList.Id);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        //#region Extensions

        //[HttpGet]
        //public HttpResponseMessage GetExtensions()
        //{
        //    try
        //    {
        //        var extensions = ExtensionRepository.Instance.GetExtensions().OrderByDescending(e => e.LastModifiedOnDate);

        //        var availableExtensions = new List<AvailableExtensionDTO>();

        //        var dir = HttpContext.Current.Server.MapPath("~/DesktopModules/BusinessEngine/install");

        //        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        //        var ext = new List<string> { "extension", "b" };
        //        var extFiles = Directory
        //            .EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
        //            .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));
        //        foreach (var filePath in extFiles)
        //        {
        //            var json = FileUtil.GetFileContent(filePath);
        //            var model = JsonConvert.DeserializeObject<AvailableExtensionDTO>(json);
        //            availableExtensions.Add(model);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            Extensions = extensions,
        //            AvailableExtensions = availableExtensions
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage InstallAvailableExtensions(AvailableExtensionDTO postData)
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
        //        var scenarioName = ScenarioRepository.Instance.GetScenarioName(scenarioId);

        //        var installPath = HttpContext.Current.Server.MapPath("~/DesktopModules/BusinessEngine/install/");

        //        var filename = (installPath + postData.ExtensionFile);
        //        var extensionUnzipedPath = this.PortalSettings.HomeSystemDirectoryMapPath + @"BusinessEngine\Temp\" + scenarioName + @"\" + Path.GetFileNameWithoutExtension(filename);
        //        var manifestFile = ExtensionService.UnzipExtensionFile(extensionUnzipedPath, filename);

        //        File.Delete(installPath + postData.ManifestFile);

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            ExtensionJson = FileUtil.GetFileContent(manifestFile),
        //            ManifestFilePath = manifestFile,
        //            ExtensionUnzipedPath = extensionUnzipedPath
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<HttpResponseMessage> UploadExtensionPackage()
        //{
        //    try
        //    {
        //        if (!this.UserInfo.IsSuperUser)
        //            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Problem!. the current user for installing extension must be superuser!");

        //        if (HttpContext.Current.Request.Files.Count > 0)
        //        {
        //            string fileName = HttpContext.Current.Request.Files[0].FileName;
        //            if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
        //                throw new Exception("File type not allowed");
        //        }

        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
        //        var scenarioName = ScenarioRepository.Instance.GetScenarioName(scenarioId);

        //        if (Request.Content.IsMimeMultipartContent())
        //        {
        //            var uploadPath = this.PortalSettings.HomeSystemDirectoryMapPath + @"BusinessEngine\Temp";
        //            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

        //            var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(uploadPath);

        //            await Request.Content.ReadAsMultipartAsync(streamProvider);

        //            string filename = streamProvider.FileData[0].LocalFileName;

        //            var extensionUnzipedPath = this.PortalSettings.HomeSystemDirectoryMapPath + @"BusinessEngine\Temp\" + scenarioName + @"\" + Path.GetFileNameWithoutExtension(filename);

        //            var manifestFile = ExtensionService.UnzipExtensionFile(extensionUnzipedPath, filename);
        //            var manifestJson = FileUtil.GetFileContent(manifestFile);
        //            var extension = JsonConvert.DeserializeObject<ExtensionManifestDTO>(manifestJson);

        //            extension.InstallTemporaryItemId = ExtensionInstallTemporaryRepository.Instance.AddItem(new ExtensionInstallTemporaryView()
        //            {
        //                ExtensionManifestJson = manifestJson,
        //                ExtensionInstallUnzipedPath = extensionUnzipedPath,
        //                IsExtensionInstalled = false,
        //                CreatedOnDate = DateTime.Now,
        //                CreatedByUserId = this.UserInfo.UserId,
        //            });

        //            return Request.CreateResponse(HttpStatusCode.OK, extension);
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

        //        var extensionController = new ExtensionService(scenarioId, this.PortalSettings, this.UserInfo);
        //        extensionController.InstallExtension(extension, objExtensionInstallTemporaryInfo.ExtensionInstallUnzipedPath, monitoringInstance);

        //        ExtensionInstallTemporaryRepository.Instance.DeleteItem(installTemporaryItemId);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeleteExtension(GuidDTO postData)
        //{
        //    try
        //    {
        //        ExtensionService.Instance.UninstallExtension(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //#endregion

        //#region Providers

        //[HttpGet]
        //public HttpResponseMessage GetProvider(Guid providerId)
        //{
        //    try
        //    {
        //        var provider = ProviderRepository.Instance.GetProvider(providerId);

        //        return Request.CreateResponse(HttpStatusCode.OK, provider);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SaveProvider(ProviderInfo provider)
        //{
        //    try
        //    {
        //        ProviderRepository.Instance.UpdateProvider(provider);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //#endregion

        //#region Libraries & Page Resources

        //[HttpGet]
        //public HttpResponseMessage GetLibraries()
        //{
        //    try
        //    {
        //        var libraries = LibraryMapping.GetLibrariesViewModel();

        //        return Request.CreateResponse(HttpStatusCode.OK, libraries);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[HttpGet]
        //public HttpResponseMessage GetPages()
        //{
        //    var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //    try
        //    {
        //        var result = new List<object>();
        //        var tabs = new List<int?>();

        //        var scenarioModules = ModuleRepository.Instance.GetModules(scenarioId).Where(m => m.ParentId == null && m.DnnModuleId != null && m.ModuleType != "Dashboard");
        //        foreach (var moduleGroup in scenarioModules.GroupBy(m => m.DnnModuleId))
        //        {
        //            var tabId = ModuleRepository.Instance.GetModuleTabId(moduleGroup.First().DnnModuleId.Value);
        //            if (tabs.Contains(tabId)) continue;
        //            tabs.Add(tabId);

        //            var tab = TabController.Instance.GetTab(tabId, PortalSettings.PortalId);
        //            var item = new { tab.TabId, tab.TabName, Modules = moduleGroup.OrderBy(m => m.ViewOrder).Select(m => new { m.ModuleId, m.ModuleType, m.ModuleName, m.ModuleTitle }) };
        //            result.Add(item);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[HttpGet]
        //public HttpResponseMessage GetPageResources(string cmsPageId)
        //{
        //    try
        //    {
        //        var pageResources = PageResourceRepository.Instance.GetPageResources(cmsPageId);

        //        var result = pageResources.GroupBy(r => r.ModuleId);

        //        return Request.CreateResponse(HttpStatusCode.OK, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SetResourceStatus(PageResourceDTO postData)
        //{
        //    try
        //    {
        //        var objPageResourceInfo = PageResourceRepository.Instance.GetPageResource(postData.ResourceId);
        //        objPageResourceInfo.IsActive = postData.IsActvive;
        //        PageResourceRepository.Instance.UpdatePageResource(objPageResourceInfo);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeletePageResource(GuidDTO postData)
        //{
        //    try
        //    {
        //        PageResourceRepository.Instance.DeletePageResource(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //#endregion

        //#region Payment Methods

        //[HttpGet]
        //public HttpResponseMessage GetPaymentMethods()
        //{
        //    try
        //    {
        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        var paymentMethods = PaymentMethodRepository.Instance.GetPaymentMethods(scenarioId);

        //        var scenarios = ScenarioRepository.Instance.GetScenarios();

        //        var providers = ProviderRepository.Instance.GetProviders("PaymentGateway");

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            PaymentMethods = paymentMethods,
        //            Scenarios = scenarios,
        //            Providers = providers,
        //        });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SavePaymentMethod(PaymentMethodInfo paymentMethod)
        //{
        //    try
        //    {
        //        var objPaymentMethod = new PaymentMethodInfo()
        //        {
        //            PaymentMethodId = paymentMethod.PaymentMethodId,
        //            ScenarioId = paymentMethod.ScenarioId,
        //            PaymentMethodName = paymentMethod.PaymentMethodName,
        //            SuccessfulPaymentTemplate = paymentMethod.SuccessfulPaymentTemplate,
        //            UnsuccessfulPaymentTemplate = paymentMethod.UnsuccessfulPaymentTemplate,
        //            Description = paymentMethod.Description,
        //            ViewOrder = paymentMethod.ViewOrder
        //        };

        //        objPaymentMethod.LastModifiedOnDate = paymentMethod.LastModifiedOnDate = DateTime.Now;
        //        objPaymentMethod.LastModifiedByUserId = paymentMethod.LastModifiedByUserId = this.UserInfo.UserId;

        //        if (paymentMethod.PaymentMethodId == Guid.Empty)
        //        {
        //            objPaymentMethod.CreatedOnDate = DateTime.Now;
        //            objPaymentMethod.CreatedByUserId = this.UserInfo.UserId;

        //            paymentMethod.PaymentMethodId = PaymentMethodRepository.Instance.AddPaymentMethod(objPaymentMethod);
        //        }
        //        else
        //        {
        //            objPaymentMethod.CreatedOnDate = paymentMethod.CreatedOnDate == DateTime.MinValue ? DateTime.Now : paymentMethod.CreatedOnDate;
        //            objPaymentMethod.CreatedByUserId = paymentMethod.CreatedByUserId;

        //            PaymentMethodRepository.Instance.UpdatePaymentMethod(objPaymentMethod);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, paymentMethod);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeletePaymentMethod(GuidDTO postData)
        //{
        //    try
        //    {
        //        PaymentMethodRepository.Instance.DeletePaymentMethod(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //#endregion

        //#region Import/Export

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage ExportScenario(ManifestModel manifest)
        //{
        //    try
        //    {
        //        manifest.PackageType = "Scenario Full Components";

        //        var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

        //        ExportWorker.ExportScenario(scenarioId, manifest.PackageName, manifest, this.PortalSettings);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<HttpResponseMessage> ImportFile()
        //{
        //    try
        //    {
        //        Enum.TryParse(HttpContext.Current.Request.Params["PackageType"], out ExportImportOperationType packageType);

        //        if (HttpContext.Current.Request.Files.Count > 0)
        //        {
        //            var fileName = HttpContext.Current.Request.Files[0].FileName;
        //            if (!Host.AllowedExtensionWhitelist.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLower()))
        //                throw new Exception("File type not allowed");
        //        }

        //        if (Request.Content.IsMimeMultipartContent())
        //        {
        //            string uploadPath = PortalSettings.HomeSystemDirectory + "business-engine/temp/";
        //            var mapPath = HttpContext.Current.Server.MapPath(uploadPath);
        //            if (!Directory.Exists(mapPath)) Directory.CreateDirectory(mapPath);

        //            var streamProvider = new CustomMultipartFormDataStreamProviderChangeFileName(mapPath);

        //            await Request.Content.ReadAsMultipartAsync(streamProvider);

        //            var file = streamProvider.FileData.FirstOrDefault();

        //            if (packageType == ExportImportOperationType.ImportScenario)
        //            {
        //                ImportWorker.ImportScenario(file.LocalFileName, this.PortalSettings, this.UserInfo, DataContext.Instance(), HttpContext.Current);
        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK);
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
        //}

        //#endregion
    }
}