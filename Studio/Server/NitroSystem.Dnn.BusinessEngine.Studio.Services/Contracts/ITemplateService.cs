using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Template;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Contracts
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync();
    }
}
