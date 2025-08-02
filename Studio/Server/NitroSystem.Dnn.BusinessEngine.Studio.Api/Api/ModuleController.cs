using DotNetNuke.Web.Api;
using NitroSystem.Dnn.BusinessEngine.Studio.Api.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
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
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using System.Web.UI;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using DotNetNuke.Security.Roles;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule;
using NitroSystem.Dnn.BusinessEngine.Core.General;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class ModuleController : DnnApiController
    {
        private readonly IGlobalService _globalService;
        private readonly IServiceFactory _serviceFactory;
        private readonly IViewModelService _viewModelService;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly ITemplateService _templateService;

        public ModuleController(
            IGlobalService globalService,
            IServiceFactory serviceFactory,
            IViewModelService viewModelService,
            IModuleService moduleService,
            IActionService actionService,
            ITemplateService templateService
        )
        {
            _globalService = globalService;
            _serviceFactory = serviceFactory;
            _viewModelService = viewModelService;
            _moduleService = moduleService;
            _actionService = actionService;
            _templateService = templateService;
        }

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
                var moduleId = await _moduleService.SaveModuleAsync(module, module.Id == Guid.Empty);

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
        public async Task<HttpResponseMessage> SaveModuleTemplate(ModuleTemplateDto module)
        {
            try
            {
                var isUpdated = await _moduleService.UpdateModuleTemplateAsync(module);

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
        public async Task<HttpResponseMessage> SortModuleCustomLibraries([FromUri] LibraryOrResource target, IEnumerable<SortDto> postData)
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
        public async Task<HttpResponseMessage> DeleteModuleCustomLibrary(GuidDto postData)
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
        public async Task<HttpResponseMessage> DeleteModuleCustomResource(GuidDto postData)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleCustomLibrary(ModuleCustomLibraryViewModel library)
        {
            try
            {
                library.Id = await _moduleService.SaveModuleCustomLibraryAsync(library);

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
                resource.Id = await _moduleService.SaveModuleCustomResourceAsync(resource);

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

                var variables = await _moduleService.GetModuleVariablesViewModelAsync(moduleId);
                var viewModels = await _viewModelService.GetViewModelsAsync(scenarioId, 1, 1000, "", "Title");

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    VariableTypes = GlobalItems.VariableTypes.Append("ViewModel").Append("ViewModelList"),
                    Variables = variables,
                    ViewModels = viewModels.Items
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
                variable.Id = await _moduleService.SaveModuleVariablesAsync(variable, variable.Id == Guid.Empty);

                return Request.CreateResponse(HttpStatusCode.OK, variable.Id);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleVariable(GuidDto postData)
        {
            try
            {
                var isDeleted = await _moduleService.DeleteModuleVariablesAsync(postData.Id);

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
                var fieldTypes = await _moduleService.GetFieldTypesViewModelAsync();
                var fields = await _moduleService.GetFieldsViewModelAsync(moduleId);
                var variables = await _moduleService.GetModuleVariablesDtoAsync(moduleId);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Module = module,
                    FieldTypes = fieldTypes,
                    Fields = fields,
                    VariablesAsList = variables.Where(v => v.Scope != ModuleVariableScope.ServerSide && v.VariableType == "ViewModelList")
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
                var field = await _moduleService.GetFieldViewModelAsync(fieldId);

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
        public async Task<HttpResponseMessage> BuildModule([FromUri] Guid moduleId)
        {
            try
            {
                await _moduleService.BuildModuleAsync(moduleId, PortalSettings, HttpContext.Current);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> SaveModuleField(UpdateModuleFieldDto postData)
        {
            try
            {
                postData.Field.Id = await _moduleService.SaveFieldAsync(postData.Field, postData.Field.Id == Guid.Empty);

                if (postData.ReorderFields && postData.PaneFieldIds != null && postData.PaneFieldIds.Any())
                {
                    if (postData.FieldViewOrder.HasValue) postData.PaneFieldIds.Insert(postData.FieldViewOrder.Value, postData.Field.Id);

                    await _moduleService.SortFieldsAsync(new SortPaneFieldsDto()
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
        public async Task<HttpResponseMessage> UpdateModuleFieldPaneAndReorderFields(SortPaneFieldsDto postData)
        {
            try
            {
                await _moduleService.UpdateFieldPaneAsync(postData);

                await _moduleService.SortFieldsAsync(new SortPaneFieldsDto()
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
        public async Task<HttpResponseMessage> SortModuleFields(SortPaneFieldsDto postData)
        {
            try
            {
                await _moduleService.SortFieldsAsync(postData);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> DeleteModuleField(GuidDto postData)
        {
            try
            {
                var isDeleted = await _moduleService.DeleteFieldAsync(postData.Id);

                return Request.CreateResponse(HttpStatusCode.OK, isDeleted);
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
                var results = await _actionService.GetActionsViewModelAsync(moduleId, fieldId, pageIndex, pageSize, searchText, actionType, sortBy);

                var actions = results.Items;
                var totalCount = results.TotalCount;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
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
        public async Task<HttpResponseMessage> GetAction(Guid moduleId, Guid? fieldId, Guid actionId, string fieldType)
        {
            try
            {
                var scenarioId = Guid.Parse(Request.Headers.GetValues("ScenarioId").First());

                var actionTypes = await _actionService.GetActionTypesViewModelAsync();
                var actions = await _actionService.GetActionsViewModelAsync(moduleId, fieldId, 1, 1000, null, null, "ActionName");
                var variables = await _moduleService.GetModuleVariablesViewModelAsync(moduleId);

                var events = Enumerable.Empty<ModuleFieldTypeCustomEventListItem>();

                if (string.IsNullOrEmpty(fieldType))
                    events = GetDefaultCustomEvents();
                else
                    events = GetDefaultCustomEvents(fieldType).Concat(await _moduleService.GetFieldTypesGetCustomEventsAsync(fieldType));

                var action = actionId != Guid.Empty
                        ? await _actionService.GetActionViewModelAsync(actionId)
                        : null;

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ActionTypes = actionTypes,
                    Actions = actions.Items,
                    Variables = variables,
                    Events = events,
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
        public async Task<HttpResponseMessage> DeleteAction(GuidDto postData)
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
