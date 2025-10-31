using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Module
{
    public class ModuleTemplateService : IModuleTemplateService
    {
        private readonly IRepositoryBase _repository;

        public ModuleTemplateService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task<ModuleTemplateViewModel> GetTemplateViewModelAsync(Guid moduleId)
        {
            var module = await _repository.GetAsync<ModuleInfo>(moduleId);

            return HybridMapper.Map<ModuleInfo, ModuleTemplateViewModel>(module);
        }

        public async Task<Guid?> GetTemplateIdAsync(Guid moduleId)
        {
            var template = await _repository.GetColumnValueAsync<ModuleInfo, string>(moduleId, "Template");
            return await _repository.GetColumnValueAsync<TemplateInfo, Guid>("Id", "TemplateName", template);
        }

        public async Task<bool> UpdateTemplateAsync(ModuleTemplateViewModel module)
        {
            var objModuleInfo = HybridMapper.Map<ModuleTemplateViewModel, ModuleInfo>(module);

            return await _repository.UpdateAsync(
                objModuleInfo,
                "Template",
                "Theme",
                "PreloadingTemplate",
                "LayoutTemplate",
                "LayoutCss"
            );
        }
    }
}