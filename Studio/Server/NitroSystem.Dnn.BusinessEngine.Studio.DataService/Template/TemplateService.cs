using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Template
{
    public class TemplateService : ITemplateService
    {
        private readonly IRepositoryBase _repository;
        private readonly IModuleService _moduleService;

        public TemplateService(IRepositoryBase repository, IModuleService moduleService)
        {
            _repository = repository;
            _moduleService = moduleService;
        }

        public async Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync(ModuleType moduleType)
        {
            var templates = await _repository.GetAllAsync<TemplateInfo>();
            var themes = await _repository.GetAllAsync<TemplateThemeInfo>();

            var builder = new CollectionMappingBuilder<TemplateInfo, TemplateViewModel>();

            builder.AddChildAsync<TemplateThemeInfo, TemplateThemeViewModel, Guid>(
               source: themes,
               parentKey: parent => parent.Id,
               childKey: child => child.TemplateId,
               assign: (dest, children) => dest.Themes = children
           );

            var result = await builder.BuildAsync(templates);
            return result;
        }
    }
}
