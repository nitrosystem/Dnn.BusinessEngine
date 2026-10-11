using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Module
{
    public class ModuleVariableService : IModuleVariableService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModuleVariableRepository _moduleVariableRepository;
        private readonly IAppModelRepository _appModelRepository;

        public ModuleVariableService(IUnitOfWork unitOfWork, IModuleVariableRepository moduleVariableRepository, IAppModelRepository appModelRepository)
        {
            _unitOfWork = unitOfWork;
            _moduleVariableRepository = moduleVariableRepository;
            _appModelRepository = appModelRepository;
        }

        public async Task<IEnumerable<ModuleVariableViewModel>> GetVariablesViewModelAsync(Guid moduleId)
        {
            var variables = await _moduleVariableRepository.GetsAsync(moduleId, "ViewOrder", "GroupName");
            return HybridMapper.MapCollection<ModuleVariableInfo, ModuleVariableViewModel>(variables);
        }

        public async Task<IEnumerable<ModuleVariableListItem>> GetVariablesListItemAsync(Guid moduleId)
        {
            var variables = await _moduleVariableRepository.GetsAsync(moduleId, "VariableName");
            var properties = await _appModelRepository.GetsAsModuleVariables(moduleId);

            return HybridMapper.MapWithChildren<ModuleVariableInfo, ModuleVariableListItem, AppModelPropertyInfo, PropertyInfo>(
                parents: variables,
                children: properties,
                parentKeySelector: p => p.AppModelId,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<Guid> CreateVariablesAsync(ModuleVariableViewModel variable)
        {
            var objModuleVariableInfo = HybridMapper.Map<ModuleVariableViewModel, ModuleVariableInfo>(variable);
            if (objModuleVariableInfo.Id == Guid.Empty)
            {
                objModuleVariableInfo.Id = await _moduleVariableRepository.AddAsync(objModuleVariableInfo);
            }
            else
            {
                await _moduleVariableRepository.UpdateAsync(objModuleVariableInfo);
            }

            return objModuleVariableInfo.Id;
        }

        public async Task SortVariablesAsync(ItemsOrder item)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                int index = 0;
                foreach (var variableId in item.SortedIds)
                {
                    var objModuleVariableInfo = new ModuleVariableInfo() { Id = variableId, ViewOrder = index++ };
                    await _moduleVariableRepository.UpdateAsync(objModuleVariableInfo, "ViewOrder");
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        public async Task<bool> DeleteVariablesAsync(Guid moduleId)
        {
            return await _moduleVariableRepository.DeleteAsync(moduleId);
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
                    var variables = JsonConvert.DeserializeObject<IReadOnlyList<ModuleVariableInfo>>(json);

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
            var variables = await _moduleVariableRepository.GetsByAncestorAsync(scenarioId);
            return variables;
        }

        private async Task<IEnumerable<ModuleVariableInfo>> GetVariablesAsync(Guid moduleId)
        {
            return await _moduleVariableRepository.GetsAsync(moduleId);
        }

        private async Task SaveVariablesAsync(Guid moduleId, IEnumerable<ModuleVariableInfo> variables)
        {
            foreach (var variable in variables)
            {
                variable.ModuleId = moduleId;
                variable.Id = Guid.NewGuid();

                await _moduleVariableRepository.AddAsync(variable);
            }
        }

        #endregion
    }
}
