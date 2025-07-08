using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IModuleService
    {
        #region Module Services

        Task<ModuleViewModel> GetModuleViewModelAsync(Guid id);

        Task<IEnumerable<PageResourceViewModel>> GetPageResourcesByModuleViewModelAsync(Guid moduleId);

        Task<IEnumerable<PageResourceViewModel>> GetActivePageResourcesByPageViewModelAsync(int pageId);

        #endregion

        #region Module Field Services

        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId);

        #endregion
    }
}
