using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync(ModuleType moduleType, Guid? parentId);
    }
}
