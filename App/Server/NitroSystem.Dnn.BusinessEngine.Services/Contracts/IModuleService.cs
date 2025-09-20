using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Contracts
{
    public interface IModuleService
    {
        #region Module Services

        Task<ModuleViewModel> GetModuleViewModelAsync(Guid moduleId);
        ModuleViewModel GetModuleViewModel(Guid moduleId);

        #endregion

        #region Module Field Services

        Task<IEnumerable<ModuleFieldViewModel>> GetFieldsViewModelAsync(Guid moduleId);
        IEnumerable<ModuleFieldViewModel> GetFieldsViewModel(Guid moduleId);

        #endregion

        #region Module Variables

        Task<IEnumerable<ModuleVariableDto>> GetModuleVariablesAsync(Guid moduleId, ModuleVariableScope scope);
        IEnumerable<ModuleVariableDto> GetModuleVariables(Guid moduleId, ModuleVariableScope scope);

        Task<IEnumerable<ModuleClientVariableDto>> GetModuleClientVariables(Guid moduleId);

        #endregion
    }
}
