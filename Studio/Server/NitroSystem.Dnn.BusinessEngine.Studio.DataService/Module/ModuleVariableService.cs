using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleVariableService : IModuleVariableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public ModuleVariableService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<IEnumerable<ModuleVariableViewModel>> GetModuleVariablesViewModelAsync(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId, "ViewOrder");

            return HybridMapper.MapCollection<ModuleVariableInfo, ModuleVariableViewModel>(variables);
        }

        public async Task<IEnumerable<ModuleVariableListItem>> GetModuleVariablesListItemAsync(Guid moduleId)
        {
            var variables = await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId, "VariableName");
            var properties = await _repository.ExecuteStoredProcedureAsListAsync<AppModelPropertyInfo>(
                "dbo.BusinessEngine_Studio_GetAppModelPropertiesAsModuleVariables", "BE_AppModelProperties_ModuleVariables_" + moduleId,
                new
                {
                    ModuleId = moduleId
                });

            return HybridMapper.MapWithChildren<ModuleVariableInfo, ModuleVariableListItem, AppModelPropertyInfo, PropertyInfo>(
                parents: variables,
                children: properties,
                parentKeySelector: p => p.AppModelId,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<Guid> SaveModuleVariablesAsync(ModuleVariableViewModel variable, bool isNew)
        {
            var objModuleVariableInfo = HybridMapper.Map<ModuleVariableViewModel, ModuleVariableInfo>(variable);

            if (isNew)
            {
                objModuleVariableInfo.Id = await _repository.AddAsync<ModuleVariableInfo>(objModuleVariableInfo, true);
            }
            else
            {
                var isUpdated = await _repository.UpdateAsync<ModuleVariableInfo>(objModuleVariableInfo);
                if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objModuleVariableInfo);
            }
            return objModuleVariableInfo.Id;
        }

        public async Task<bool> DeleteModuleVariablesAsync(Guid moduleId)
        {
            return await _repository.DeleteAsync<ModuleVariableInfo>(moduleId);
        }
    }
}
