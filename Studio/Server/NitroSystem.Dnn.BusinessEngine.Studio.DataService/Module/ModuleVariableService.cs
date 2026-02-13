using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleVariableService : IModuleVariableService, IExportable, IImportable
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
                objModuleVariableInfo.Id = await _repository.AddAsync<ModuleVariableInfo>(objModuleVariableInfo);
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

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var itemss = await GetScenarioVariablesAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = itemss,
                        IsSuccess = true
                    };
                case ImportExportScope.Module:
                    var variables = await GetVariablesAsync(context.Get<Guid>("ModuleId"));

                    return new ExportResponse()
                    {
                        Result = variables,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.Module:
                    var moduleId = (Guid)context.DataTrack["ModuleId"];
                    var variables = JsonConvert.DeserializeObject<IEnumerable<ModuleVariableInfo>>(json);

                    await SaveVariablesAsync(moduleId, variables);

                    return new ImportResponse()
                    {
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        private async Task<IEnumerable<ModuleVariableInfo>> GetScenarioVariablesAsync(Guid scenarioId)
        {
            var variables = new List<ModuleVariableInfo>();

            var moduleIds = await _repository.GetColumnsValueAsync<ModuleInfo, Guid>("Id", "ScenarioId", scenarioId);
            foreach (var moduleId in moduleIds)
            {
                variables.AddRange(await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId));
            }

            return variables;
        }

        private async Task<IEnumerable<ModuleVariableInfo>> GetVariablesAsync(Guid moduleId)
        {
            return await _repository.GetByScopeAsync<ModuleVariableInfo>(moduleId);
        }

        private async Task SaveVariablesAsync(Guid moduleId, IEnumerable<ModuleVariableInfo> variables)
        {
            foreach (var variable in variables)
            {
                variable.ModuleId = moduleId;
                variable.Id = Guid.NewGuid();

                await _repository.AddAsync<ModuleVariableInfo>(variable);
            }
        }

        #endregion
    }
}
