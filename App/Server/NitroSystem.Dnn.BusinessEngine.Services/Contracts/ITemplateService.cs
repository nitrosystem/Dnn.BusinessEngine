using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.Providers;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface ITemplateService
    {
        Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync(ModuleType moduleType);

        Task<TemplateViewModel> GetTemplateViewModelAsync(Guid id);
    }
}
