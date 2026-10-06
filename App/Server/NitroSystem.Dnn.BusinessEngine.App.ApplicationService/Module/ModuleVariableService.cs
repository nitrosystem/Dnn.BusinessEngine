using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Module
{
    public class ModuleVariableService : IModuleVariableService
    {
        private readonly IModuleVariableRepository _moduleVariableRepository;

        public ModuleVariableService(IModuleVariableRepository moduleVariableRepository)
        {
            _moduleVariableRepository = moduleVariableRepository;
        }

        public async Task<IEnumerable<ModuleVariableDto>> GetVariablesAsync(
            Guid moduleId,
            ModuleVariableScope fromScope,
            ModuleVariableScope toScope)
        {
            var results = await _moduleVariableRepository.GetsAsync(moduleId, fromScope, toScope);
            return HybridMapper.MapWithChildren<ModuleVariableSpResult, ModuleVariableDto, AppModelPropertyInfo, PropertyInfo>(
               parents: results.Variables,
               children: results.Properties,
               parentKeySelector: p => p.AppModelId,
               childKeySelector: c => c.AppModelId,
               assignChildren: (parent, childs) => parent.Properties = childs
           );
        }

        public async Task<IEnumerable<ModuleClientVariableDto>> GetClientVariablesAsync(Guid moduleId)
        {
            var variables = await _moduleVariableRepository.GetsAsync(moduleId);
            return HybridMapper.MapCollection<ModuleVariableInfo, ModuleClientVariableDto>(variables);
        }
    }
}
