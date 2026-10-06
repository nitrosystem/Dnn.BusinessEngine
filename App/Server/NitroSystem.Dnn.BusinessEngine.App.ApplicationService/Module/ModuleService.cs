using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.App.ApplicationService.Module
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IModuleOutputResourceRepository _moduleOutputResourceRepository;

        public ModuleService(IModuleRepository moduleRepository, IModuleOutputResourceRepository moduleOutputResourceRepository)
        {
            _moduleRepository = moduleRepository;
            _moduleOutputResourceRepository = moduleOutputResourceRepository;
        }

        public async Task<ModuleDto> GetModuleViewModelAsync(Guid moduleId)
        {
            var module = await _moduleRepository.GetViewAsync(moduleId);
            return HybridMapper.Map<ModuleView, ModuleDto>(module);
        }

        public async Task<string> GetModuleNameAsync(Guid moduleId)
        {
            return await _moduleRepository.GetModuleNameAsync(moduleId);
        }

        public async Task<string> GetScenarioNameAsync(Guid moduleId)
        {
            return await _moduleRepository.GetScenarioNameAsync(moduleId);
        }

        public ModuleLiteDto GetModuleLiteData(int? siteModuleId, Guid? moduleId = null)
        {
            var module = _moduleRepository.GetLiteData(siteModuleId, moduleId);
            return HybridMapper.Map<ModuleLiteSpResult, ModuleLiteDto>(module);
        }

        public IReadOnlyList<ModuleOutputResourceDto> GetModuleOutputResources(int sitePageId, string moduleIds)
        {
            var resources = _moduleOutputResourceRepository.Gets(sitePageId, moduleIds);
            return HybridMapper.MapCollection<ModuleOutputResourceInfo, ModuleOutputResourceDto>(resources);
        }
    }
}
