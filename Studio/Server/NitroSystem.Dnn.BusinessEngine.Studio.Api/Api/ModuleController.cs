using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Action;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Dashboard;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundJob;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class ModuleController : DnnApiController
    {
        private readonly IBaseService _baseService;
        private readonly IAppModelService _appModelServices;
        private readonly IServiceFactory _serviceFactory;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly IModuleFieldService _moduleFieldService;
        private readonly IModuleVariableService _moduleVariableService;
        private readonly IModuleLibraryAndResourceService _moduleLibraryAndResourceService;
        private readonly IActionService _actionService;
        private readonly ITemplateService _templateService;
        private readonly IBuildModuleRunner _buildModuleRunner;

        public ModuleController(
            IBaseService globalService,
            IAppModelService appModelService,
            IServiceFactory serviceFactory,
            IDashboardService dashboardService,
            IModuleService moduleService,
            IModuleFieldService moduleFieldService,
            IModuleVariableService moduleVariableService,
            IModuleLibraryAndResourceService moduleLibraryAndResourceService,
            IActionService actionService,
            ITemplateService templateService,
            BackgroundJobWorker backgroundJobWorker,
            IBuildModuleRunner buildModuleRunner
        )
        {
            _baseService = globalService;
            _appModelServices = appModelService;
            _serviceFactory = serviceFactory;
            _dashboardService = dashboardService;
            _moduleService = moduleService;
            _moduleFieldService = moduleFieldService;
            _moduleVariableService = moduleVariableService;
            _moduleLibraryAndResourceService = moduleLibraryAndResourceService;
            _actionService = actionService;
            _templateService = templateService;
            _buildModuleRunner = buildModuleRunner;
        }

        #region Create Dashboard

        #region 1-Basic Options

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardBasicOptions(Guid? moduleId = null)
        {
            try
            {
                var dashboard = moduleId.HasValue
                    ? await _dashboardService.GetDashboardViewModelAsync(moduleId.Value)
                    : null;
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Dashboard = dashboard
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDashboardBasicInfo(DashboardViewModel dashboard)
        {
            try
            {
                var result = await _dashboardService.CreateDashboardAsync(dashboard);
                return Request.CreateResponse(HttpStatusCode.OK, new { result.DashboardId, result.ModuleId });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardPage(Guid moduleId, Guid? pageId = null)
        {
            try
            {
                var dashboardId = await _dashboardService.GetDashboardIdAsync(moduleId);
                var pages = await _dashboardService.GetDashboardPagesListItemAsync(moduleId);
                var page = pageId.HasValue
                    ? await _dashboardService.GetDashboardPageViewModelAsync(pageId.Value)
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDashboardPage(DashboardPageViewModel page)
        {
            try
            {
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                var result = await _dashboardService.CreateDashboardPageAsync(page, UserInfo.UserID, basePath);

                //Check & Rebuild page module
                if (page.PageType == DashboardPageType.Standard && page.IncludeModule && page.Module?.ModuleId != Guid.Empty)
                    await _moduleService.EnqueueRebuildIfRequiredAsync(page.Module.ScenarioId, page.Module.ModuleId, UserInfo.UserID, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SortDashboardPages(ItemsOrder dashboard)
        {
            try
            {
                await _dashboardService.SortDashboardPagesAsync(dashboard);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdatePageParent(UpdateDashboardPageParent page)
        {
            try
            {
                await _dashboardService.UpdatePageParentAsync(page);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteDashboardPage(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _dashboardService.DeletePageAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #endregion

        #region Create Module

        #region 1-Basic Options

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBasicOptions(Guid? moduleId = null)
        {
            try
            {
                var module = moduleId.HasValue
                    ? await this._moduleService.GetModuleViewModelAsync(moduleId.Value, string.Empty)
                    : null;
                var fieldTypeExists = module != null ||
                    await _moduleFieldService.HasFieldTypeAsync();

                return Request.CreateResponse(HttpStatusCode.OK, new { Module = module, FieldTypeExists = fieldTypeExists });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //[HttpGet]
        //public async Task<HttpResponseMessage> IsValidModuleName(Guid scenarioId, string moduleName)
        //{
        //    try
        //    {
        //        //var result = await _moduleService.IsValidModuleNameAsync(scenarioId, moduleName);
        //        return Request.CreateResponse(HttpStatusCode.OK, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleBasicOptions(ModuleViewModel module)
        {
            try
            {
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                Guid moduleId = await _moduleService.CreateModuleAsync(module, UserInfo.UserID, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, moduleId);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModule(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleService.DeleteModuleAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region 2-Select Template

        [HttpGet]
        public async Task<HttpResponseMessage> GetTemplates(Guid moduleId)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId, string.Empty);
                var parentId = module.ParentId.HasValue
                    ? await _moduleService.GetTemplateIdAsync(module.ParentId.Value)
                    : null;
                var templates = await _templateService.GetTemplatesViewModelAsync(module.Wrapper, module.ModuleType, parentId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    Templates = templates
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleTemplate(ModuleTemplateViewModel module)
        {
            try
            {
                var modulePath = await _moduleService.GetModulePathAsync(module.ScenarioId, module.Id, module.ParentId, module.ModuleName, PortalSettings.HomeSystemDirectoryMapPath);
                await FileUtil.WriteFileContentAsync(modulePath + "_layout.html", module.LayoutTemplate);
                await FileUtil.WriteFileContentAsync(modulePath + "_layout.css", module.LayoutCss);
                if (!string.IsNullOrWhiteSpace(module.PreloadingTemplate))
                    await FileUtil.WriteFileContentAsync(modulePath + "_preloader.html", module.PreloadingTemplate);

                await _moduleService.UpdateTemplateAsync(module);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                var moduleCustomLibraries = await _moduleLibraryAndResourceService.GetCustomLibrariesViewModelAsync(moduleId);
                var moduleCustomResources = await _moduleLibraryAndResourceService.GetCustomResourcesViewModelAsync(moduleId);

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
        public async Task<HttpResponseMessage> SortModuleCustomLibraries([FromUri] LibraryOrResource target, ItemsOrder postData)
        {
            try
            {
                if (target == LibraryOrResource.Library)
                    await _moduleLibraryAndResourceService.SortCustomLibrariesAsync(postData);
                else if (target == LibraryOrResource.Resource)
                    await _moduleLibraryAndResourceService.SortCustomResourcesAsync(postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomLibrary([FromUri] Guid scenarioId, ModuleCustomLibraryViewModel library)
        {
            try
            {
                library.Id = await _moduleLibraryAndResourceService.CreateCustomLibraryAsync(library);

                //Check & Rebuild page module
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                await _moduleService.EnqueueRebuildIfRequiredAsync(scenarioId, library.ModuleId, UserInfo.UserID, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, library.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomResource([FromUri] Guid scenarioId, ModuleCustomResourceViewModel resource)
        {
            try
            {
                resource.Id = await _moduleLibraryAndResourceService.CreateCustomResourceAsync(resource);

                //Check & Rebuild page module
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                await _moduleService.EnqueueRebuildIfRequiredAsync(scenarioId, resource.ModuleId, UserInfo.UserID, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, resource.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleCustomLibrary([FromUri] Guid scenarioId, GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleLibraryAndResourceService.DeleteCustomLibraryAsync(postData.Id);

                //Check & Rebuild page module
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                await _moduleService.EnqueueRebuildIfRequiredAsync(scenarioId, postData.ParentId, UserInfo.UserID, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleCustomResource([FromUri] Guid scenarioId, GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleLibraryAndResourceService.DeleteCustomResourceAsync(postData.Id);

                //Check & Rebuild page module
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                await _moduleService.EnqueueRebuildIfRequiredAsync(scenarioId, postData.ParentId, UserInfo.UserID, basePath);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region 4-Variables

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleVariables(Guid scenarioId, Guid moduleId)
        {
            try
            {
                //Get Variables view model for management
                var variables = await _moduleVariableService.GetVariablesViewModelAsync(moduleId);

                //Get App Models for variables that their type is AppModel/AppModelList
                var appModels = await _appModelServices.GetAppModelsListItemAsync(scenarioId, "ModelName");

                //Get system type in c#
                var variableTypes = Constants.VariableTypes.Select(vt => new { Text = vt.Key, Value = vt.Value });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    VariableTypes = variableTypes,
                    Variables = variables,
                    AppModels = appModels
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleVariablesForModuleBuilder(Guid moduleId)
        {
            try
            {
                var variables = await _moduleVariableService.GetVariablesListItemAsync(moduleId);
                return Request.CreateResponse(HttpStatusCode.OK, variables);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleVariable(ModuleVariableViewModel variable)
        {
            try
            {
                variable.Id = await _moduleVariableService.CreateVariablesAsync(variable);
                return Request.CreateResponse(HttpStatusCode.OK, variable.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SortModuleVariables(ItemsOrder postData)
        {
            try
            {
                await _moduleVariableService.SortVariablesAsync(postData);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleVariable(GuidInfo postData)
        {
            try
            {
                var isDeleted = await _moduleVariableService.DeleteVariablesAsync(postData.Id);
                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region 5-Module Builder

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBuilder(Guid moduleId)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId, PortalSettings.HomeSystemDirectoryMapPath);
                var fieldTypes = await _moduleFieldService.GetFieldTypesViewModelAsync();
                var fields = await _moduleFieldService.GetFieldsViewModelAsync(moduleId);
                var variables = await _moduleVariableService.GetVariablesListItemAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    FieldTypes = fieldTypes,
                    Fields = fields,
                    Variables = variables.Where(v => v.Scope != ModuleVariableScope.ServerSide)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleField(ModuleFieldUpdatedItems postData)
        {
            try
            {
                postData.Field.Id = await _moduleFieldService.CreateFieldAsync(postData.Field);

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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateModuleFieldPaneAndReorderFields(PaneFieldsOrder postData)
        {
            try
            {
                await _moduleFieldService.UpdateFieldPaneAsync(postData);
                await _moduleFieldService.SortFieldsAsync(postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                string basePath = PortalSettings.HomeSystemDirectoryMapPath + @"business-engine\";
                request.BasePath = basePath;
                request.UserId = UserInfo.UserID;
                request.Module = await _moduleService.GetDataForModuleBuildingAsync(moduleId);
                var result = await _buildModuleRunner.RunAsync(request);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region 6-Actions

        [HttpGet]
        public async Task<HttpResponseMessage> GetActions(
            Guid moduleId,
            Guid? fieldId = null,
            string searchText = null,
            string actionType = null)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId, PortalSettings.HomeSystemDirectoryMapPath);
                var fileds = await _moduleFieldService.GetFieldsListItemAsync(moduleId, "FieldName");
                var actionTypes = await _actionService.GetActionTypesListItemAsync("ActionType", "ViewOrder");
                var results = await _actionService.GetActionsViewModelAsync(moduleId, fieldId, searchText, actionType);
                var actions = results.Items;
                var totalCount = results.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    Fields = fileds,
                    ActionTypes = actionTypes,
                    Actions = actions
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetFieldActionsForModuleBuilder(Guid moduleId, Guid fieldId)
        {
            try
            {
                var actions = await _actionService.GetActionsListItemAsync(moduleId, fieldId);
                return Request.CreateResponse(HttpStatusCode.OK, actions);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAction(Guid moduleId, Guid? actionId = null, Guid? fieldId = null, string fieldType = "")
        {
            try
            {
                var actionTypes = await _actionService.GetActionTypesListItemAsync("ActionDomain");
                var actions = await _actionService.GetActionsListItemAsync(moduleId, fieldId, "ActionName");
                var variables = await _moduleVariableService.GetVariablesListItemAsync(moduleId);
                var action = actionId.HasValue
                            ? await _actionService.GetActionViewModelAsync(actionId.Value)
                            : null;
                var events = string.IsNullOrEmpty(fieldType)
                            ? await _moduleService.GetModuleEventTypesListItem()
                            : await _moduleService.GetModuleEventTypesListItem(fieldType);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ActionTypes = actionTypes,
                    Actions = actions,
                    Variables = variables,
                    Events = events,
                    Action = action,
                    DslBuiltInFunctions = ExpressionFunctions.BuiltIn.Keys
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveAction(ActionViewModel action)
        {
            try
            {
                var validationResults = new Dictionary<string, object>();
                string message = string.Empty;
                Guid? actionId = null;
                ActionDto existsAction = null;

                if (action.Id == Guid.Empty)
                {
                    var item = await _actionService.ValidateActionExecuteOrderAsync(action.ModuleId, action.FieldId, action.ParentId, action.Event, action.ExecuteOrder);
                    if (item != null)
                    {
                        existsAction = item;
                        validationResults.Add("DuplicateExecuteOrderValidation",
                            $@"This ""Execute Order"" has already been saved by ""{item.ActionName}"" Action,
                            Please enter another Execute Order.");
                    }
                }

                if (!string.IsNullOrEmpty(action.ActionConditionsDsl) && !IsValidDslScript(action.ActionConditionsDsl, out message))
                    validationResults.Add("ActionConditionsDslValidation", message);
                if (!string.IsNullOrEmpty(action.ActionConditionsDsl) && action.ActionConditionsDsl.IndexOf("_ConditionIsTrue") == -1)
                    validationResults.Add("ConditionIsTrueNotFound", true);

                if (!string.IsNullOrEmpty(action.BeforeExecuteActionDsl) && !IsValidDslScript(action.BeforeExecuteActionDsl, out message))
                    validationResults.Add("BeforeExecuteActionDslValidation", message);

                if (!string.IsNullOrEmpty(action.ActionResultsDsl) && !IsValidDslScript(action.ActionResultsDsl, out message))
                    validationResults.Add("ActionResultsDslValidation", message);

                if (validationResults.Count == 0)
                    actionId = await _actionService.SaveActionAsync(action);

                var serviceType = action?.ServiceId.HasValue == true
                    ? await _serviceFactory.GetServiceTypeNameAsync(action.ServiceId.Value)
                    : "";

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ActionId = actionId,
                    ServiceType = serviceType,
                    ValidationResults = validationResults,
                    ExistsAction = existsAction
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private bool IsValidDslScript(string script, out string message)
        {
            try
            {
                var tokenizer = new Tokenizer(script);
                List<Token> tokens = tokenizer.Tokenize();

                var parser = new DslParser(tokens);
                parser.ParseScript();

                message = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        #endregion
    }
}
