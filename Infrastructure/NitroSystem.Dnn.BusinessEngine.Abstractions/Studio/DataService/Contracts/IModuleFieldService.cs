using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IModuleFieldService
    {
        Task<IEnumerable<ModuleFieldTypeViewModel>> GetFieldTypesViewModelAsync();
        Task<IEnumerable<ModuleFieldTypeCustomEventListItem>> GetFieldTypesCustomEventsListItemAsync(string fieldType);
        Task<string> GetFieldTypeIconAsync(string fieldType);


        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleID, string sortBy = "ViewOrder");
        Task<ModuleFieldViewModel> GetFieldViewModelAsync(Guid fieldId);
        Task<Guid> SaveFieldAsync(ModuleFieldViewModel field, bool isNew);
        Task<bool> UpdateFieldPaneAsync(PaneFieldsOrder data);
        Task SortFieldsAsync(PaneFieldsOrder data);
        Task<bool> DeleteFieldAsync(Guid moduleId);
    }
}
