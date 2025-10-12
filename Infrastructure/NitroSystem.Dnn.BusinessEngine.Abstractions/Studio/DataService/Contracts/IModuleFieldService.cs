using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.Contracts
{
    public interface IModuleFieldService
    {
        Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync();
        Task<IEnumerable<ModuleFieldTypeCustomEventListItem>> GetFieldTypesCustomEventsListItemAsync(string fieldType);
        Task<string> GetFieldTypeIconAsync(string fieldType);


        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleID, string sortBy = "ViewOrder");
        Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId);
        Task<Guid> SaveFieldAsync(ModuleFieldViewModel field, bool isNew);
        Task<bool> UpdateFieldPaneAsync(SortPaneFieldsDto data);
        Task SortFieldsAsync(SortPaneFieldsDto data);
        Task<bool> DeleteFieldAsync(Guid moduleId);
    }
}
