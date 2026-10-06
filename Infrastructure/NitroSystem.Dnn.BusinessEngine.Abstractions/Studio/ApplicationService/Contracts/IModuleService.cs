using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IModuleService
    {
        #region Module

        Task<string> GetModuleNameAsync(Guid moduleId);
        Task<string> GetModulePathAsync(Guid scenarioId, Guid moduleId, Guid? parentModuleId, string moduleName, string basePath);
        string GetModulePath(string scenarioName, string parentModuleName, string moduleName, string basePath);
        Task<IEnumerable<ModuleViewModel>> GetModulesViewModelAsync(Guid scenarioId);
        Task<List<ModuleEventTypeListItem>> GetModuleEventTypesListItem(string fieldType = null);
        Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId, string basePath);

        Task<Guid> CreateModuleAsync(ModuleViewModel module, int userId, string basePath);

        Task<bool> DeleteModuleAsync(Guid moduleId);

        Task EnqueueRebuildIfRequiredAsync(Guid scenarioId, Guid moduleId, int userId, string basePath);
        void RenameModuleFolder(string oldModuleName, string newModuleName, string parentModuleName, string scenarioName, string basePath);

        #endregion

        #region Module Template

        Task<ModuleTemplateViewModel> GetTemplateViewModelAsync(Guid moduleId);
        Task<Guid?> GetTemplateIdAsync(Guid moduleId);
        Task UpdateTemplateAsync(ModuleTemplateViewModel module);

        #endregion

        #region Build Module

        Task<ModuleDto> GetDataForModuleBuildingAsync(Guid moduleId);
        Task<bool> IsRebuildRequiredAsync(Guid moduleId);
        Task DeleteModuleResourcesAsync(Guid moduleId);
        Task BulkInsertModuleOutputResourcesAsync(int? sitePageId, IEnumerable<ModuleResourceDto> resources);
        #endregion
    }
}
