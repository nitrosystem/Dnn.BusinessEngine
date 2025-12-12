using System;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.BackgroundTask;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Enums;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class ModuleController : DnnApiController
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICacheService _cacheService;
        private readonly IBaseService _baseService;
        private readonly IAppModelService _appModelServices;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly IModuleTemplateService _moduleTemplateService;
        private readonly IModuleFieldService _moduleFieldService;
        private readonly IModuleVariableService _moduleVariableService;
        private readonly IModuleLibraryAndResourceService _moduleLibraryAndResourceService;
        private readonly IActionService _actionService;
        private readonly ITemplateService _templateService;
        private readonly WorkflowManager _workflow;
        private readonly BackgroundFramework _backgroundFramework;

        public ModuleController(
            IServiceProvider serviceProvider,
            ICacheService cacheService,
            IBaseService globalService,
            IAppModelService appModelService,
            IDashboardService dashboardService,
            IModuleService moduleService,
            IModuleTemplateService moduleTemplateService,
            IModuleFieldService moduleFieldService,
            IModuleVariableService moduleVariableService,
            IModuleLibraryAndResourceService moduleLibraryAndResourceService,
            IActionService actionService,
            ITemplateService templateService,
            WorkflowManager workflow,
            BackgroundFramework backgroundFramework
        )
        {
            _serviceProvider = serviceProvider;
            _cacheService = cacheService;
            _baseService = globalService;
            _appModelServices = appModelService;
            _dashboardService = dashboardService;
            _moduleService = moduleService;
            _moduleTemplateService = moduleTemplateService;
            _moduleFieldService = moduleFieldService;
            _moduleVariableService = moduleVariableService;
            _moduleLibraryAndResourceService = moduleLibraryAndResourceService;
            _actionService = actionService;
            _templateService = templateService;
            _workflow = workflow;
            _backgroundFramework = backgroundFramework;
        }

        #region Create Dashboard

        #region 1-Basic Options

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardBasicOptions(Guid moduleId)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardViewModelAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Dashboard = dashboard
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDashboardBasicInfo(DashboardViewModel dashboard)
        {
            try
            {
                var result = await _dashboardService.SaveDashboardAsync(dashboard);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { DashboardId = result.Item1, ModuleId = result.Item2 });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 2-Dashboard Pages

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardPages(Guid moduleId)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardViewModelAsync(moduleId);
                var pages = await _dashboardService.GetDashboardPagesViewModelAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Dashboard = dashboard,
                    Pages = pages,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardPage(Guid moduleId)
        {
            return await GetDashboardPage(moduleId, Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardPage(Guid moduleId, Guid pageId)
        {
            try
            {
                var dashboardId = await _dashboardService.GetDashboardIdAsync(moduleId);
                var pages = await _dashboardService.GetDashboardPagesListItemAsync(moduleId);
                var page = pageId != Guid.Empty
                    ? await _dashboardService.GetDashboardPageViewModelAsync(pageId)
                    : null;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    DashboardId = dashboardId,
                    Pages = pages,
                    Page = page,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDashboardPage(DashboardPageViewModel page)
        {
            try
            {
                bool rebuild = false;
                if (page.PageType == DashboardPageType.Standard && page.IncludeModule)
                    rebuild = await _moduleService.IsRebuildRequired(page.Module.ModuleId, page.Module.ModuleName);

                var result = await _dashboardService.SaveDashboardPageAsync(page);

                if (rebuild)
                {
                    var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                    var scenarioName = await _baseService.GetScenarioNameAsync(scenarioId);
                    var basePath = $"{PortalSettings.HomeSystemDirectory}business-engine/{StringHelper.ToKebabCase(scenarioName)}/";
                    var job = new BuildModuleTask(_serviceProvider, _cacheService, _moduleService, _workflow, page.Module.ModuleId, UserInfo.UserID, basePath);
                    var req = new BackgroundTaskRequest(job, TaskPriority.Normal, scenarioName);

                    _backgroundFramework.Enqueue(req);
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SortDashboardPages(DashboardPagesOrder dashboard)
        {
            try
            {
                await _dashboardService.SortDashboardPages(dashboard);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdatePageParent(UpdateDashboardPageParent page)
        {
            try
            {
                await _dashboardService.UpdatePageParent(page);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteDashboardPage(GuidInfo postData)
        {
            try
            {
                await _dashboardService.DeletePageAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        ////public HttpResponseMessage DeleteDashboardPageModule(GuidDTO postData)
        ////{
        ////    try
        ////    {
        ////        DashboardPageModuleRepository.Instance.DeleteModuleByModuleId(postData.Id);
        ////        ModuleRepository.Instance.DeleteModule(postData.Id);

        ////        return Request.CreateResponse(HttpStatusCode.OK);
        ////    }
        ////    catch (Exception ex)
        ////    {

        ////        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        ////    }
        ////}

        //#endregion

        //#region 4-Dashboard Modules

        //[HttpGet]
        //public async Task<HttpResponseMessage> GetDashboardModules(Guid moduleId)
        //{
        //    try
        //    {
        //        var modules = await _dashboardService.GetDashboardPagesModule(moduleId);

        //        return Request.CreateResponse(HttpStatusCode.OK, modules);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //#endregion

        #endregion

        #endregion

        #region Create Module

        #region 1-Basic Options

        [HttpGet]
        public HttpResponseMessage GetModuleBasicOptions()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBasicOptions(Guid moduleId)
        {
            try
            {
                var module = await this._moduleService.GetModuleViewModelAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, module);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> CheckModuleName(CheckModuleNameDto module)
        {
            try
            {
                var result = await _moduleService.IsValidModuleNameAsync(module.ScenarioId, module.ModuleId, module.ModuleName);

                return Request.CreateResponse(HttpStatusCode.OK, result ?? false);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleBasicOptions(ModuleViewModel module)
        {
            try
            {
                var isNew = module.Id == Guid.Empty;
                var rebuild = !isNew
                    ? await _moduleService.IsRebuildRequired(module.Id, module.ModuleName)
                    : false;

                var moduleId = await _moduleService.SaveModuleAsync(module, isNew);

                if (rebuild)
                {
                    var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                    var scenarioName = await _baseService.GetScenarioNameAsync(scenarioId);
                    var basePath = $"{PortalSettings.HomeSystemDirectory}business-engine/{StringHelper.ToKebabCase(scenarioName)}/";
                    var job = new BuildModuleTask(_serviceProvider, _cacheService, _moduleService, _workflow, module.Id, UserInfo.UserID, basePath);
                    var req = new BackgroundTaskRequest(job, TaskPriority.Normal, scenarioName);

                    _backgroundFramework.Enqueue(req);
                }

                return Request.CreateResponse(HttpStatusCode.OK, moduleId);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 2-Select Template

        [HttpGet]
        public async Task<HttpResponseMessage> GetTemplates(Guid moduleId)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId);
                var parentId = module.ParentId.HasValue
                    ? await _moduleTemplateService.GetTemplateIdAsync(module.ParentId.Value)
                    : null;
                var templates = await _templateService.GetTemplatesViewModelAsync(module.ModuleType, parentId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    Templates = templates
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleTemplate(ModuleTemplateViewModel module)
        {
            try
            {
                var isUpdated = await _moduleTemplateService.UpdateTemplateAsync(module);

                return Request.CreateResponse(HttpStatusCode.OK, isUpdated);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 3-Libraries & Resources

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleCustomLibraries(Guid moduleId)
        {
            try
            {
                var libraries = await _baseService.GetLibrariesListItemAsync();
                var moduleCustomLibraries = await _moduleLibraryAndResourceService.GetModuleCustomLibrariesAsync(moduleId);
                var moduleCustomResources = await _moduleLibraryAndResourceService.GetModuleCustomResourcesAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Libraries = libraries,
                    ModuleCustomLibraries = moduleCustomLibraries,
                    ModuleCustomResources = moduleCustomResources
                });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SortModuleCustomLibraries([FromUri] LibraryOrResource target, IEnumerable<SortInfo> postData)
        {
            try
            {
                await _moduleLibraryAndResourceService.SortModuleCustomLibraries(target, postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleCustomLibrary(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleLibraryAndResourceService.DeleteModuleCustomLibraryAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleCustomResource(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleLibraryAndResourceService.DeleteModuleCustomResourceAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomLibrary(ModuleCustomLibraryViewModel library)
        {
            try
            {
                library.Id = await _moduleLibraryAndResourceService.SaveModuleCustomLibraryAsync(library, library.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, library.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomResource(ModuleCustomResourceViewModel resource)
        {
            try
            {
                resource.Id = await _moduleLibraryAndResourceService.SaveModuleCustomResourceAsync(resource, resource.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, resource.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 4-Variables

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleVariables(Guid moduleId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var variables = await _moduleVariableService.GetModuleVariablesViewModelAsync(moduleId);
                var appModels = await _appModelServices.GetAppModelsAsync(scenarioId, 1, 1000, "", "Title");

                var types = Constants.VariableTypes;
                types["AppModel"] = "AppModel";
                types["AppModelList"] = "AppModelList";
                var variableTypes = types.Select(kvp => new { Text = kvp.Key, Value = kvp.Value });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    VariableTypes = variableTypes,
                    Variables = variables,
                    AppModels = appModels.Items
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleVariable(ModuleVariableViewModel variable)
        {
            try
            {
                variable.Id = await _moduleVariableService.SaveModuleVariablesAsync(variable, variable.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, variable.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleVariable(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleVariableService.DeleteModuleVariablesAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 5-Module Builder

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBuilder(Guid moduleId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var module = await _moduleService.GetModuleViewModelAsync(moduleId);
                var actions = await _actionService.GetActionsListItemAsync(moduleId);
                var fieldTypes = await _moduleFieldService.GetFieldTypesViewModelAsync();
                var fields = await _moduleFieldService.GetFieldsViewModelAsync(moduleId);
                var variables = await _moduleVariableService.GetModuleVariablesListItemAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    Actions = actions,
                    FieldTypes = fieldTypes,
                    Fields = fields,
                    Variables = variables.Where(v => v.Scope != ModuleVariableScope.ServerSide)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleField(Guid fieldId)
        {
            try
            {
                var field = await _moduleFieldService.GetFieldViewModelAsync(fieldId);

                return Request.CreateResponse(HttpStatusCode.OK, field);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetModuleTemplateContent(string templatePath)
        {
            try
            {
                templatePath = templatePath.Replace("[ModulePath]", "DesktopModules/BusinessEngine");
                var templateContent = FileUtil.GetFileContent(HttpContext.Current.Server.MapPath($"~/{templatePath}"));

                return Request.CreateResponse(HttpStatusCode.OK, templateContent);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleField(ModuleFieldUpdatedItems postData)
        {
            try
            {
                postData.Field.Id = await _moduleFieldService.SaveFieldAsync(postData.Field, postData.Field.Id == Guid.Empty);

                if (postData.ReorderFields && postData.PaneFieldIds != null && postData.PaneFieldIds.Any())
                {
                    if (postData.FieldViewOrder.HasValue) postData.PaneFieldIds.Insert(postData.FieldViewOrder.Value, postData.Field.Id);

                    await _moduleFieldService.SortFieldsAsync(new PaneFieldsOrder()
                    {
                        ModuleId = postData.Field.ModuleId,
                        PaneName = postData.Field.PaneName,
                        PaneFieldIds = postData.PaneFieldIds
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, postData.Field.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateModuleFieldPaneAndReorderFields(PaneFieldsOrder postData)
        {
            try
            {
                await _moduleFieldService.UpdateFieldPaneAsync(postData);

                await _moduleFieldService.SortFieldsAsync(new PaneFieldsOrder()
                {
                    ModuleId = postData.ModuleId,
                    PaneName = postData.PaneName,
                    PaneFieldIds = postData.PaneFieldIds
                });

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SortModuleFields(PaneFieldsOrder postData)
        {
            try
            {
                await _moduleFieldService.SortFieldsAsync(postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleField(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleFieldService.DeleteFieldAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 5-1 Build Module

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> BuildModule([FromUri] Guid moduleId)
        {
            try
            {
                var request = new BuildModuleRequest();
                request.Scope = BuildScope.Module;
                request.UserId = UserInfo.UserID;
                request.Module = await _workflow.ExecuteTaskAsync<ModuleDto>(moduleId.ToString(), UserInfo.UserID,
                    "BuildModuleWorkflow", "BuildModule", "GetDataForBuildModule", false, true, false,
                    () => _moduleService.GetDataForModuleBuildingAsync(moduleId)
                 );
                request.BasePath = $"{PortalSettings.HomeSystemDirectory}business-engine/{StringHelper.ToKebabCase(request.Module.ScenarioName)}/";

                var engine = new BuildModuleEngine(_serviceProvider, _cacheService, _moduleService, _workflow, true);
                await engine.ExecuteAsync(request);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 6-Actions

        [HttpGet]
        public async Task<HttpResponseMessage> GetActions(
            Guid moduleId, int pageIndex, int pageSize, Guid? fieldId = null, string searchText = null, string actionType = null, string sortBy = "Newest")
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId);
                var results = await _actionService.GetActionsViewModelAsync(moduleId, fieldId, pageIndex, pageSize, searchText, actionType, sortBy);
                var actions = results.Items;
                var totalCount = results.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    Actions = actions,
                    Page = new PagingInfo(totalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAction(Guid moduleId)
        {
            return await GetAction(moduleId, Guid.Empty, string.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAction(Guid moduleId, string fieldType)
        {
            return await GetAction(moduleId, null, Guid.Empty, fieldType);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAction(Guid moduleId, Guid? fieldId, string fieldType)
        {
            return await GetAction(moduleId, fieldId, Guid.Empty, fieldType);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAction(Guid moduleId, Guid actionId)
        {
            return await GetAction(moduleId, null, actionId, string.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAction(Guid moduleId, Guid? fieldId, Guid actionId, string fieldType)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var actionTypes = await _actionService.GetActionTypesListItemAsync();
                var actions = await _actionService.GetActionsViewModelAsync(moduleId, fieldId, 1, 1000, null, null, "ActionName");
                var variables = await _moduleVariableService.GetModuleVariablesListItemAsync(moduleId);
                var events = Enumerable.Empty<ModuleFieldTypeCustomEventListItem>();
                var actionKeys = await _actionService.GetActionKeysAsync();
                var action = actionId != Guid.Empty
                        ? await _actionService.GetActionViewModelAsync(actionId)
                        : null;

                if (string.IsNullOrEmpty(fieldType))
                    events = GetDefaultCustomEvents();
                else
                    events = GetDefaultCustomEvents(fieldType).Concat(await _moduleFieldService.GetFieldTypesCustomEventsListItemAsync(fieldType));

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ActionTypes = actionTypes,
                    Actions = actions.Items,
                    Variables = variables,
                    Events = events,
                    ActionKeys = actionKeys,
                    Action = action,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveAction(ActionViewModel action)
        {
            try
            {
                action.Id = await _actionService.SaveActionAsync(action, action.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, action.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteAction(GuidInfo postData)
        {
            try
            {
                var result = await _actionService.DeleteActionAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private IEnumerable<ModuleFieldTypeCustomEventListItem> GetDefaultCustomEvents(string fieldType = "")
        {
            return !string.IsNullOrEmpty(fieldType)
                ? new[]
                {
                    new ModuleFieldTypeCustomEventListItem { Text = "On Action Completed", Value = "OnActionCompleted" },
                    new ModuleFieldTypeCustomEventListItem { Text = "On Field Value Change", Value = "OnFieldValueChange" },
                    new ModuleFieldTypeCustomEventListItem { Text = "On Field Custom Event", Value = "OnCustomEvent" },
                }
                : new[]
                {
                    new ModuleFieldTypeCustomEventListItem { Text = "On Page Init", Value = "OnPageInit" },
                    new ModuleFieldTypeCustomEventListItem { Text = "On Page Load", Value = "OnPageLoad" },
                    new ModuleFieldTypeCustomEventListItem { Text = "On Action Completed", Value = "OnActionCompleted" },
                    new ModuleFieldTypeCustomEventListItem { Text = "On Payment Completed", Value = "OnPaymentCompleted" },
                    new ModuleFieldTypeCustomEventListItem { Text = "On Custom Event", Value = "OnCustomEvent" },
                };
        }

        #endregion

        #region 7-publish

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SortModuleCustomResources([FromUri] ModuleCustomResourcesType target, IEnumerable<ModuleCustomResourceDTO> postData)
        //{
        //    try
        //    {
        //        if (target == ModuleCustomResourcesType.CustomLibrary)
        //            ModuleCustomLibraryRepository.Instance.SortItems(postData.ToJson());

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<HttpResponseMessage> DeleteCustomLibrary(GuidDTO postData)
        //{
        //    try
        //    {
        //        var isDeleted = await _dashboardService.DeleteLibraryAsync(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<HttpResponseMessage> DeleteCustomResource(GuidDTO postData)
        //{
        //    try
        //    {
        //        var isDeleted = await _dashboardService.DeleteResourceAsync(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion

        #endregion
    }
}
