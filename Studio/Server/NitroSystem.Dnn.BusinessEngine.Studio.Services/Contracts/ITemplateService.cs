using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Template;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync();
    }
}
