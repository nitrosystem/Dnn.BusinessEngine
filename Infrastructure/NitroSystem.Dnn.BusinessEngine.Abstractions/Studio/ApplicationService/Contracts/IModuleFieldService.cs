using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IModuleFieldService
    {
        #region Field Type

        Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync();
        Task<string> GetFieldTypeIconAsync(string fieldType);
        Task<string> GetGeneratePanesBusinessControllerClassAsync(string fieldType);
        Task<bool> HasFieldTypeAsync();

        #endregion

        #region Field

        Task<string> GetFieldTypeAsync(Guid fieldId);
        Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId);
        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleID, string sortBy = "ViewOrder");
        Task<IEnumerable<ModuleFieldListItem>> GetFieldsListItemAsync(Guid moduleID, string sortBy);

        Task<Guid> CreateFieldAsync(ModuleFieldViewModel field);
        Task<bool> UpdateFieldPaneAsync(PaneFieldsOrder data);
        Task SortFieldsAsync(PaneFieldsOrder data);

        Task<bool> DeleteFieldAsync(Guid moduleId);

        #endregion
    }
}
