using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Template;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync(ModuleWrapper moduleWrapper, ModuleType moduleType, Guid? parentId);
    }
}
