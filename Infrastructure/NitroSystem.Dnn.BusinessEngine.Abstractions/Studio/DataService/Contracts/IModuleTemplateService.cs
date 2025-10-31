using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface IModuleTemplateService
    {
        Task<ModuleTemplateViewModel> GetTemplateViewModelAsync(Guid moduleId);
        Task<Guid?> GetTemplateIdAsync(Guid moduleId);
        Task<bool> UpdateTemplateAsync(ModuleTemplateViewModel module);
    }
}
