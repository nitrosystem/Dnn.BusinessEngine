using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Template;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.Template
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly ITemplateThemeRepository _templateThemeRepository;

        public TemplateService(ITemplateRepository templateRepository, ITemplateThemeRepository templateThemeRepository)
        {
            _templateRepository = templateRepository;
            _templateThemeRepository = templateThemeRepository;
        }

        public async Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync(ModuleWrapper moduleWrapper, ModuleType moduleType, Guid? parentId)
        {
            var templates = await _templateRepository.GetsAsync("TemplateName");
            var themes = await _templateThemeRepository.GetsAsync("ViewOrder");

            var builder = new CollectionMappingBuilder<TemplateInfo, TemplateViewModel>();
            builder.AddChildAsync<TemplateThemeInfo, TemplateThemeViewModel, Guid>(
               source: themes,
               parentKey: parent => parent.Id,
               childKey: child => child.TemplateId,
               assign: (dest, children) => dest.Themes = children
           );

            var result = await builder.BuildAsync(templates
                            .Where(t => (moduleType == ModuleType.Dashboard && t.ModuleType == 0) ||
                                        (moduleType != ModuleType.Dashboard && moduleWrapper == ModuleWrapper.DnnPage && t.ParentId == null && (t.ModuleType == null || t.ModuleType.Value == (int)moduleType)) ||
                                        (moduleType != ModuleType.Dashboard && moduleWrapper == ModuleWrapper.Dashboard && parentId.HasValue && t.ParentId == parentId && (t.ModuleType == null || t.ModuleType.Value == (int)moduleType)))
                                    );
            return result;
        }
    }
}
