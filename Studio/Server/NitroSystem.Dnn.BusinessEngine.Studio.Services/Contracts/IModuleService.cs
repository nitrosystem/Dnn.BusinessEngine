using System;
using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ListItems;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IModuleService
    {
        #region Module Services

        Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId, PortalSettings portalSettings);
        Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew);
        Task<bool> DeleteModuleAsync(Guid moduleId);
        Task<bool?> IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName);
        Task BuildModuleAsync(Guid moduleId, PortalSettings portalSettings, HttpContext context);

        #endregion

        #region Module Template Services

        Task<ModuleTemplateDto> GetModuleTemplateDtoAsync(Guid moduleId);
        Task<bool> UpdateModuleTemplateAsync(ModuleTemplateDto module);

        #endregion

        #region Module Variable Services

        Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId);
        Task<IEnumerable<ModuleVariableDto>> GetModuleVariablesDtoAsync(Guid moduleId);
        Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variale, bool isNew);
        Task<bool> DeleteModuleVariablesAsync(Guid moduleId);

        #endregion

        #region Module Field Type Services

        Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync();
        Task<IEnumerable<ModuleFieldTypeCustomEventListItem>> GetFieldTypesCustomEventsListItemAsync(string fieldType);
        Task<string> GetFieldTypeIconAsync(string fieldType);

        #endregion

        #region Module Field Services

        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleID, string sortBy = "ViewOrder");
        Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId);
        Task<Guid> SaveFieldAsync(ModuleFieldViewModel field, bool isNew);
        Task<bool> UpdateFieldPaneAsync(SortPaneFieldsDto data);
        Task SortFieldsAsync(SortPaneFieldsDto data);
        Task<bool> DeleteFieldAsync(Guid moduleId);

        #endregion

        #region Module Libraries & Resources

        Task<IEnumerable<ModuleCustomLibraryViewModel>> GetModuleCustomLibrariesAsync(Guid moduleId);
        Task<IEnumerable<ModuleCustomResourceViewModel>> GetModuleCustomResourcesAsync(Guid moduleId);
        Task<Guid> SaveModuleCustomLibraryAsync(ModuleCustomLibraryViewModel library, bool isNew);
        Task<Guid> SaveModuleCustomResourceAsync(ModuleCustomResourceViewModel resource, bool isNew);
        Task SortModuleCustomLibraries(LibraryOrResource target, IEnumerable<SortInfo> items);
        Task<bool> DeleteModuleCustomLibraryAsync(Guid moduleId);
        Task<bool> DeleteModuleCustomResourceAsync(Guid moduleId);

        #endregion
    }
}
