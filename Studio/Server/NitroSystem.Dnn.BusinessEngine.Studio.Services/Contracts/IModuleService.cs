using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface IModuleService
    {
        #region Module Services

        Task<ModuleViewModel> GetModuleViewModelAsync(Guid id, PortalSettings portalSettings);

        Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId, PortalSettings portalSettings);

        Task<ModuleBuilderType> GetModuleBuilderType(Guid moduleId);

        Task<Guid> SaveModuleAsync(ModuleViewModel module, bool isNew);

        Task<bool> DeleteModuleAsync(Guid id);

        Task<bool?> IsValidModuleName(Guid scenarioId, Guid? moduleId, string moduleName);

        Task BuildModuleAsync(BuildModuleRequest postData, PortalSettings portalSettings, HttpContext context);

        #endregion

        #region Module Variable Services

        Task<IEnumerable<VariableTypeViewModel>> GetVariableTypesViewModelAsync();

        Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId);

        Task<ModuleVariableViewModel> GetModuleVariableViewModelAsync(Guid variableId);

        Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variale, bool isNew);

        Task<bool> DeleteModuleVariablesAsync(Guid id);

        #endregion

        #region Module Field Type Services

        Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync();

        #endregion

        #region Module Field Services

        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleID);

        Task<Guid> SaveModuleFieldAsync(ModuleFieldViewModel field, bool isNew);

        Task<bool> UpdateModuleFieldPaneAsync(SortPaneFieldsDto data);

        Task SortModuleFieldsAsync(SortPaneFieldsDto data);

        Task<bool> DeleteModuleFieldAsync(Guid id);

        Task<bool> UpdateModuleLayoutTemplateAsync(ModuleLayoutTemplateDto data);

        #endregion

        #region Module Libraries & Resources

        Task<IEnumerable<ModuleCustomLibraryViewModel>> GetModuleCustomLibrariesAsync(Guid moduleId);

        Task<IEnumerable<ModuleCustomResourceViewModel>> GetModuleCustomResourcesAsync(Guid moduleId);

        Task<Guid> SaveModuleCustomLibraryAsync(ModuleCustomLibraryDto library);

        Task<Guid> SaveModuleCustomResourceAsync(ModuleCustomResourceDto resource);

        Task SortModuleCustomLibraries(LibraryOrResource target, IEnumerable<SortModuleCustomLibrariesDto> items);

        Task<bool> DeleteModuleCustomLibraryAsync(Guid id);

        Task<bool> DeleteModuleCustomResourceAsync(Guid id);

        #endregion
    }
}
