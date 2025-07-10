using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Common;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.WebSocketServer;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using System.Web.UI;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using DotNetNuke.Security.Roles;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class ModuleController : DnnApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IGlobalService _globalService;
        private readonly IDashbaordService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly ITemplateService _templateService;

        public ModuleController(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IGlobalService globalService,
            IDashbaordService dashboardService,
            IModuleService moduleService,
            IActionService actionService,
            ITemplateService templateService
        )
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _globalService = globalService;
            _dashboardService = dashboardService;
            _moduleService = moduleService;
            _actionService = actionService;
            _templateService = templateService;
        }

        #region Create Module

        //#region Modules & Fields

        #region 1-Basic Options

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBasicOptions()
        {
            return await GetModuleBasicOptions(Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBasicOptions(Guid moduleId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var module = moduleId == Guid.Empty
                    ? null
                    : await this._moduleService.GetModuleViewModelAsync(moduleId, this.PortalSettings);

                var scenarios = this._globalService.GetScenariosViewModelAsync();

                IEnumerable<string> roles = null;
                roles = RoleController.Instance.GetRoles(PortalSettings.PortalId).Cast<RoleInfo>().Select(r => r.RoleName);
                var allUsers = new List<string>();
                allUsers.Add("All Users");
                roles = allUsers.Concat(roles);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    Scenarios = scenarios,
                    Roles = roles,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> CheckModuleName(CheckModuleNameDTO module)
        {
            try
            {
                var result = await _moduleService.IsValidModuleName(module.ScenarioId, module.ModuleId, module.ModuleName);

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
                _unitOfWork.BeginTransaction();

                var moduleId = await _moduleService.SaveModuleAsync(module, module.Id == Guid.Empty);

                _unitOfWork.Commit();

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
        public async Task<HttpResponseMessage> GetTemplates(Guid moduleId, string searchText = "")
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(moduleId, PortalSettings);
                var installedTemplates = await _templateService.GetTemplatesViewModelAsync(module.ModuleType);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    InstalledTemplates = installedTemplates
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleTemplate(ModuleTemplateDTO postData)
        {
            try
            {
                var module = await _moduleService.GetModuleViewModelAsync(postData.ModuleId, PortalSettings);
                module.Template = postData.Template;
                module.Theme = postData.Theme;
                module.LayoutTemplate = postData.LayoutTemplate;
                module.LayoutCss = postData.LayoutCss;

                _unitOfWork.BeginTransaction();

                await _moduleService.SaveModuleAsync(module, false);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 3-Variables

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleVariables(Guid moduleId)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                var viewModelService = new ViewModelService(_unitOfWork, _cacheService);

                var variableTypes = await _moduleService.GetVariableTypesViewModelAsync();
                var variables = await _moduleService.GetModuleVariablesViewModelAsync(moduleId);
                var viewModels = await viewModelService.GetViewModelsAsync(scenarioId, 1, 1000, "", "Title");

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    VariableTypes = variableTypes,
                    Variables = variables,
                    ViewModels = viewModels
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
                bool isNew = variable.Id == Guid.Empty;

                variable.Id = await _moduleService.SaveModuleVariablesAsync(variable, isNew);

                variable = await _moduleService.GetModuleVariableViewModelAsync(variable.Id);

                return Request.CreateResponse(HttpStatusCode.OK, variable);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleVariable(GuidDTO postData)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var isDeleted = await _moduleService.DeleteModuleVariablesAsync(postData.Id);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 4-Libraries & Resources

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleCustomLibraries(Guid moduleId)
        {
            try
            {
                var libraries = await _globalService.GetLibrariesLiteDtoAsync();
                var moduleCustomLibraries = await _moduleService.GetModuleCustomLibrariesAsync(moduleId);
                var moduleCustomResources = await _moduleService.GetModuleCustomResourcesAsync(moduleId);

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
        public async Task<HttpResponseMessage> SortModuleCustomLibraries([FromUri] LibraryOrResource target, IEnumerable<SortModuleCustomLibrariesDto> postData)
        {
            try
            {
                await _moduleService.SortModuleCustomLibraries(target, postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleCustomLibrary(GuidDTO postData)
        {
            try
            {
                var isDeleted = await _moduleService.DeleteModuleCustomLibraryAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleCustomResource(GuidDTO postData)
        {
            try
            {
                var isDeleted = await _moduleService.DeleteModuleCustomResourceAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardLibraries(Guid moduleId)
        {
            try
            {
                var task1 = _globalService.GetLibrariesLiteDtoAsync();
                var task2 = _dashboardService.GetDashboardLibraries(moduleId);
                var task3 = _dashboardService.GetDashboardResources(moduleId);

                await Task.WhenAll(task1, task2, task3);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Libraries = (await task1).OrderBy(l => l.LibraryName),
                    DashboardLibraries = (await task2).OrderBy(l => l.LoadOrder),
                    DashboardResources = (await task3).OrderBy(l => l.LoadOrder)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetLibraryResources(Guid libraryId)
        {
            try
            {
                var result = await _globalService.GetLibraryResourcesViewModelAsync(libraryId);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomLibrary(ModuleCustomLibraryDto library)
        {
            try
            {
                library.Id = await _dashboardService.SaveCustomLibrary(library);

                return Request.CreateResponse(HttpStatusCode.OK, library.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomResource(ModuleCustomResourceDto resource)
        {
            try
            {
                resource.Id = await _dashboardService.SaveCustomResource(resource);

                return Request.CreateResponse(HttpStatusCode.OK, resource.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        #endregion

        #region 5-Module Builder

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBuilderType(Guid moduleId)
        {
            try
            {
                var moduleBuilderType = await _moduleService.GetModuleBuilderType(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, moduleBuilderType);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleBuilder(Guid moduleId, Guid? dashboardModuleId = null)
        {
            try
            {
                var ws = new WebSocketServer();
                await ws.ConnectAsync();
                await ws.SendMessageToClientAsync(10, "Starting to Get Module...");

                await ws.SendMessageToClientAsync("Get module scenario...", 5);
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                await ws.SendMessageToClientAsync("Get module data...", 15);
                var module = await _moduleService.GetModuleViewModelAsync(moduleId, PortalSettings);

                await ws.SendMessageToClientAsync("Get field types...", 45);
                var fieldTypes = await _moduleService.GetFieldTypesViewModelAsync();

                await ws.SendMessageToClientAsync("Get module fields data...", 65);
                var fields = await _moduleService.GetFieldsViewModelAsync(moduleId);

                await ws.SendMessageToClientAsync("Get module variables...", 70);
                var variables = _moduleService.GetModuleVariablesViewModelAsync(moduleId);

                await ws.CloseAsync();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    FieldTypes = fieldTypes,
                    Fields = fields
                });
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SaveModule([FromBody] ModuleViewModel module, [FromUri] bool includeAllOptions = true)
        //{
        //    try
        //    {
        //        var objModuleInfo = ModuleRepository.Instance.GetModule(module.Id) ?? new ModuleInfo();
        //        ReflectionExtensions.CopyProperties(module, objModuleInfo);
        //        objModuleInfo.PortalId = PortalSettings.PortalId;
        //        objModuleInfo.LayoutTemplate = module.ModuleBuilderType == "HtmlEditor" ? string.Empty : (includeAllOptions ? module.LayoutTemplate : objModuleInfo.LayoutTemplate);
        //        objModuleInfo.LayoutCss = module.ModuleBuilderType == "HtmlEditor" ? string.Empty : (includeAllOptions ? module.LayoutCss : objModuleInfo.LayoutCss);
        //        objModuleInfo.LastModifiedOnDate = DateTime.Now;
        //        objModuleInfo.LastModifiedByUserId = this.UserInfo.UserId;
        //        objModuleInfo.Settings = module.Settings != null && module.Settings.Count > 0 ? JsonConvert.SerializeObject(module.Settings) : null;

        //        if (objModuleInfo.ModuleId == Guid.Empty)
        //        {
        //            objModuleInfo.CreatedOnDate = DateTime.Now;
        //            objModuleInfo.CreatedByUserId = this.UserInfo.UserId;

        //            objModuleInfo.ModuleId = module.Id = ModuleRepository.Instance.AddModule(objModuleInfo);
        //        }
        //        else
        //        {
        //            ModuleRepository.Instance.UpdateModule(objModuleInfo);
        //        }

        //        DataCache.ClearCache("BEModule_" + objModuleInfo.ModuleId);
        //        DataCache.ClearCache("BEModuleFieldsView_" + objModuleInfo.ModuleId);

        //        return Request.CreateResponse(HttpStatusCode.OK, objModuleInfo.ModuleId);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleLayoutTemplate(ModuleLayoutTemplateDto postData)
        {
            try
            {
                var isUpdated = await _moduleService.UpdateModuleLayoutTemplateAsync(postData);

                return Request.CreateResponse(HttpStatusCode.OK, isUpdated);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> BuildModule(BuildModuleRequest postdata)
        {
            try
            {
                await _moduleService.BuildModuleAsync(postdata, PortalSettings, HttpContext.Current);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleField(ModuleFieldUpdateDto postData)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                postData.Field.Id = await _moduleService.SaveModuleFieldAsync(postData.Field, postData.Field.Id == Guid.Empty);

                if (postData.ReorderFields && postData.PaneFieldIds != null && postData.PaneFieldIds.Any())
                {
                    if (postData.FieldViewOrder.HasValue) postData.PaneFieldIds.Insert(postData.FieldViewOrder.Value, postData.Field.Id);

                    await _moduleService.SortModuleFieldsAsync(new SortPaneFieldsDto()
                    {
                        ModuleId = postData.Field.ModuleId,
                        PaneName = postData.Field.PaneName,
                        PaneFieldIds = postData.PaneFieldIds
                    });
                }

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, postData.Field.Id);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateModuleFieldPaneAndReorderFields(SortPaneFieldsDto postData)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                await _moduleService.UpdateModuleFieldPaneAsync(postData);

                await _moduleService.SortModuleFieldsAsync(new SortPaneFieldsDto()
                {
                    ModuleId = postData.ModuleId,
                    PaneName = postData.PaneName,
                    PaneFieldIds = postData.PaneFieldIds
                });

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SortModuleFields(SortPaneFieldsDto postData)
        {
            try
            {
                await _moduleService.SortModuleFieldsAsync(postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleField(GuidDTO postData)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var isDeleted = await _moduleService.DeleteModuleFieldAsync(postData.Id);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

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
                var results = await _actionService.GetActionsViewModelAsync(moduleId, fieldId, pageIndex, pageSize, searchText, actionType, sortBy);

                var actions = results.Items;
                var totalCount = results.TotalCount;

                var fields = await _actionService.GetFieldsHaveActionsAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Actions = actions,
                    Fields = fields,
                    Page = new PagingInfo(totalCount, pageSize, pageIndex)
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpGet]
        //public HttpResponseMessage GetFieldActions(Guid parentId)
        //{
        //    try
        //    {
        //        var actions = ActionMapping.GetFieldActionsViewModel(parentId);

        //        var moduleId = ModuleFieldRepository.Instance.GetModuleId(parentId);
        //        var module = ModuleRepository.Instance.GetModule(moduleId);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { Actions = actions, Module = module });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpGet]
        //public HttpResponseMessage GetAction(bool isFieldActions, Guid parentId, Guid? id = null)
        //{
        //    try
        //    {
        //        var moduleId = !isFieldActions ? parentId : ModuleFieldRepository.Instance.GetModuleId(parentId);
        //        var module = ModuleRepository.Instance.GetModule(moduleId);

        //        var scenarioId = ModuleRepository.Instance.GetModuleScenarioId(moduleId);

        //        var actions = isFieldActions
        //            ? ActionMapping.GetFieldActionsViewModel(parentId)
        //            : ActionMapping.GetActionsViewModel(moduleId);

        //        var action = id == null ? null : ActionMapping.GetActionViewModel(id.Value);

        //        var allActions = ActionRepository.Instance.GetModuleActions(moduleId);

        //        var fields = ModuleFieldMappings.GetFieldsViewModel(moduleId, null);

        //        var customEvents = new List<FieldTypeCustomEventInfo>();
        //        if (isFieldActions)
        //        {
        //            var fieldType = ModuleFieldRepository.Instance.GetFieldType(parentId);
        //            var fieldEventTypes = TypeCastingUtil<IEnumerable<FieldTypeCustomEventInfo>>.TryJsonCasting(ModuleFieldTypeRepository.Instance.GetCustomEvents(fieldType));
        //            if (fieldEventTypes != null) customEvents.AddRange(fieldEventTypes);
        //        }

        //        var variables = ModuleVariableRepository.Instance.GetVariables(moduleId);

        //        var viewModels = ViewModelMapping.GetViewModelsViewModel(scenarioId);

        //        var paymentMethods = PaymentMethodMapping.GetPaymentMethodsViewModel(scenarioId);

        //        var paymentGateways = ProviderRepository.Instance.GetProviders("PaymentGateway");

        //        var actionTypes = ActionMapping.GetActionTypesViewModel();

        //        return Request.CreateResponse(HttpStatusCode.OK, new
        //        {
        //            Module = module,
        //            Actions = actions,
        //            Action = action,
        //            AllActions = allActions,
        //            ActionTypes = actionTypes,
        //            Fields = fields,
        //            CustomEvents = customEvents,
        //            ViewModels = viewModels,
        //            Variables = variables,
        //            PaymentMethods = paymentMethods,
        //            PaymentGateways = paymentGateways,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        ////[HttpGet]
        ////public HttpResponseMessage GetAction(Guid parentId, Guid id)
        ////{
        ////    try
        ////    {
        ////        ActionViewModel action = ActionMapping.GetActionViewModel(id);

        ////        var moduleId = action != null & action.FieldId != null ? action.ModuleId : parentId;
        ////        var module = ModuleRepository.Instance.GetModule(moduleId);

        ////        var scenarioId = ModuleRepository.Instance.GetModuleScenarioId(moduleId);

        ////        var actions = action != null & action.FieldId != null ? ActionMapping.GetFieldActionsViewModel(action.FieldId.Value) : ActionMapping.GetActionsViewModel(parentId);

        ////        var allActions = ActionRepository.Instance.GetModuleActions(moduleId);

        ////        var fields = ModuleFieldMappings.GetFieldsViewModel(moduleId, null);

        ////        var variables = ModuleVariableRepository.Instance.GetVariables(moduleId);

        ////        var services = ServiceMapping.GetServicesViewModel(scenarioId);

        ////        var viewModels = ViewModelMapping.GetViewModelsViewModel(scenarioId);

        ////        var paymentMethods = PaymentMethodMapping.GetPaymentMethodsViewModel(scenarioId);

        ////        var paymentGateways = ProviderRepository.Instance.GetProviders("PaymentGateway");

        ////        var actionTypes = ActionMapping.GetActionTypesViewModel();

        ////        var customEvents = new List<FieldTypeCustomEventInfo>();
        ////        if (action != null & action.FieldId != null)
        ////        {
        ////            var fieldType = ModuleFieldRepository.Instance.GetFieldType(action.FieldId.Value);
        ////            var fieldEventTypes = TypeCastingUtil<IEnumerable<FieldTypeCustomEventInfo>>.TryJsonCasting(ModuleFieldTypeRepository.Instance.GetCustomEvents(fieldType));
        ////            if (fieldEventTypes != null) customEvents.AddRange(fieldEventTypes);
        ////        }

        ////        return Request.CreateResponse(HttpStatusCode.OK, new
        ////        {
        ////            Module = module,
        ////            Action = action,
        ////            Actions = actions,
        ////            AllActions = allActions,
        ////            ActionTypes = actionTypes,
        ////            Fields = fields,
        ////            CustomEvents = customEvents,
        ////            Services = services,
        ////            ViewModels = viewModels,
        ////            Variables = variables,
        ////            PaymentMethods = paymentMethods,
        ////            PaymentGateways = paymentGateways,
        ////        });
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        ////    }
        ////}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SaveAction(ActionViewModel action)
        //{
        //    try
        //    {
        //        var objActionInfo = new ActionInfo()
        //        {
        //            ActionId = action.ActionId,
        //            ParentId = action.Event == "OnActionCompleted" ? action.ParentId : null,
        //            ModuleId = action.ModuleId,
        //            FieldId = action.FieldId,
        //            ServiceId = action.ServiceId,
        //            ActionName = action.ActionName,
        //            ActionType = action.ActionType,
        //            Event = action.Event,
        //            IsServerSide = action.IsServerSide,
        //            ParentResultStatus = (byte?)action.ParentResultStatus,
        //            HasPreScript = action.HasPreScript,
        //            HasPostScript = action.HasPostScript,
        //            DisableConditionForPreScript = action.DisableConditionForPreScript,
        //            CheckConditionsInClientSide = action.CheckConditionsInClientSide,
        //            PreScript = action.PreScript,
        //            PostScript = action.PostScript,
        //            Settings = action.Settings != null && action.Settings.Count > 0 ? JsonConvert.SerializeObject(action.Settings) : null,
        //            Description = action.Description,
        //            ViewOrder = action.ViewOrder,
        //        };

        //        objActionInfo.LastModifiedOnDate = action.LastModifiedOnDate = DateTime.Now;
        //        objActionInfo.LastModifiedByUserId = action.LastModifiedByUserId = this.UserInfo.UserId;

        //        if (action.ActionId == Guid.Empty)
        //        {
        //            objActionInfo.CreatedOnDate = DateTime.Now;
        //            objActionInfo.CreatedByUserId = this.UserInfo.UserId;

        //            action.ActionId = ActionRepository.Instance.AddAction(objActionInfo);
        //        }
        //        else
        //        {
        //            objActionInfo.CreatedOnDate = action.CreatedOnDate == DateTime.MinValue ? DateTime.Now : action.CreatedOnDate;
        //            objActionInfo.CreatedByUserId = action.CreatedByUserId;

        //            ActionRepository.Instance.UpdateAction(objActionInfo);
        //        }

        //        ActionParamRepository.Instance.DeleteParams(action.ActionId);

        //        foreach (var objActionParamInfo in action.Params ?? Enumerable.Empty<ActionParamInfo>())
        //        {
        //            objActionParamInfo.ActionId = action.ActionId;

        //            ActionParamRepository.Instance.AddParam(objActionParamInfo);
        //        }

        //        ActionResultRepository.Instance.DeleteResults(action.ActionId);

        //        foreach (var item in action.Results ?? Enumerable.Empty<ActionResultViewModel>())
        //        {
        //            var objActionResultInfo = new ActionResultInfo()
        //            {
        //                ResultId = item.ResultId,
        //                ActionId = action.ActionId,
        //                LeftExpression = item.LeftExpression,
        //                EvalType = item.EvalType,
        //                RightExpression = item.RightExpression,
        //                ExpressionParsingType = item.ExpressionParsingType,
        //                GroupName = item.GroupName,
        //                Conditions = Newtonsoft.Json.JsonConvert.SerializeObject(item.Conditions)
        //            };

        //            ActionResultRepository.Instance.AddResult(objActionResultInfo);
        //        }

        //        ActionConditionRepository.Instance.DeleteConditions(action.ActionId);

        //        foreach (var objActionConditionInfo in action.Conditions ?? Enumerable.Empty<ActionConditionInfo>())
        //        {
        //            objActionConditionInfo.ActionId = action.ActionId;

        //            ActionConditionRepository.Instance.AddCondition(objActionConditionInfo);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, action);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeleteAction(GuidDTO postData)
        //{
        //    try
        //    {
        //        ActionRepository.Instance.DeleteAction(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

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

        //#region 7-Preview & Publish
        //#endregion

        //#endregion

        #endregion

        #region Create Dashboard

        #region 1-Basic Options

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardBasicOptions(Guid moduleId)
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardDtoAsync(moduleId);

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
        public async Task<HttpResponseMessage> SaveDashboardBasicInfo(DashboardDto dashboard)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var result = await _dashboardService.SaveDashboardBasicInfoAsync(dashboard);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { DashboardId = result.Item1, ModuleId = result.Item2 });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

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
                var pages = await _dashboardService.GetDashboardPagesViewModelAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Pages = pages,
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardPage(Guid dashboardModuleId)
        {
            return await GetDashboardPage(dashboardModuleId, Guid.Empty);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardPage(Guid dashboardModuleId, Guid pageId)
        {
            try
            {
                var task1 = _dashboardService.GetDashboardIdAsync(dashboardModuleId);
                var task2 = _dashboardService.GetDashboardPageDtoAsync(pageId);
                var task3 = _dashboardService.GetDashboardPagesLiteDtoAsync(dashboardModuleId);

                await Task.WhenAll(task1, task2, task3);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    DashboardId = await task1,
                    Page = await task2,
                    Pages = await task3
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDashboardPage(DashboardPageDto page)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());
                page.ScenarioId = scenarioId;

                _unitOfWork.BeginTransaction();

                var result = await _dashboardService.SaveDashboardPageAsync(page);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage SortDashboardPages(DashboardPageSortDTO postData)
        //{
        //    try
        //    {
        //        if (postData.MovedPage != null)
        //        {
        //            var objDashboardPageInfo = DashboardPageRepository.Instance.GetPage(postData.MovedPage.PageId);
        //            objDashboardPageInfo.ParentId = postData.MovedPage.ParentId;
        //            objDashboardPageInfo.ViewOrder = postData.MovedPage.ViewOrder;
        //            DashboardPageRepository.Instance.UpdatePage(objDashboardPageInfo);
        //        }

        //        int index = 0;
        //        foreach (var pageId in postData.SortedPageIds ?? Enumerable.Empty<Guid>())
        //        {
        //            var objDashboardPageInfo = DashboardPageRepository.Instance.GetPage(pageId);
        //            objDashboardPageInfo.ViewOrder = index++;
        //            DashboardPageRepository.Instance.UpdatePage(objDashboardPageInfo);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeleteDashboardPage(DashboardPageViewModel page)
        //{
        //    try
        //    {
        //        DashboardPageRepository.Instance.DeletePage(page.PageId);

        //        var pages = DashboardMapping.GetDashboardPagesViewModel(page.DashboardId, Guid.Empty);

        //        return Request.CreateResponse(HttpStatusCode.OK, pages);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public HttpResponseMessage DeleteDashboardPageModule(GuidDTO postData)
        //{
        //    try
        //    {
        //        DashboardPageModuleRepository.Instance.DeleteModuleByModuleId(postData.Id);
        //        ModuleRepository.Instance.DeleteModule(postData.Id);

        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}

        #endregion

        #region 3-Dashboard Appearance

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardAppearance(Guid moduleId)
        {
            try
            {
                var results = await _dashboardService.GetDashboardAppearanceAsync(moduleId, HttpContext.Current);

                var dashboard = results.Item1;
                var skins = results.Item2;
                var templates = results.Item3;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Skins = skins,
                    Dashboard = dashboard,
                    Templates = templates
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardTemplates(DashboardType dashboardType, string skinName, string skinPath)
        {
            try
            {
                var templates = await _dashboardService.GetDashboardTemplatesDtoAsync(dashboardType, skinName, skinPath, HttpContext.Current);

                return Request.CreateResponse(HttpStatusCode.OK, templates);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveDashboardAppearance(DashboardAppearanceDto dashboard)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                await _dashboardService.SaveDashboardAppearanceAsync(dashboard, HttpContext.Current);

                _unitOfWork.Commit();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetModuleTemplates(Guid dashboardModuleId, ModuleType moduleType)
        {
            try
            {
                var templates = await _dashboardService.GetModuleTemplates(dashboardModuleId, moduleType, HttpContext.Current);

                return Request.CreateResponse(HttpStatusCode.OK, templates);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region 4-Dashboard Modules

        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardModules(Guid moduleId)
        {
            try
            {
                var modules = await _dashboardService.GetDashboardPagesModule(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, modules);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #endregion
    }
}
