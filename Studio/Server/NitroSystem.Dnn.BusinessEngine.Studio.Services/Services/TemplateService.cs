using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Template;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
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

        public async Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync()
        {
            var task1 = _repository.GetAllAsync<TemplateInfo>();
            var task2 = _repository.GetAllAsync<TemplateThemeInfo>();
            var task3 = _repository.GetAllAsync<TemplateItemInfo>();

            await Task.WhenAll(task1, task2, task3);

            var templates = await task1;
            var themes = await task2;
            var items = await task3;

            var builder = new CollectionMappingBuilder<TemplateInfo, TemplateViewModel>();

            builder.AddChildAsync<TemplateThemeInfo, TemplateThemeViewModel, Guid>(
               source: themes,
               parentKey: parent => parent.Id,
               childKey: child => child.TemplateId,
               assign: (dest, children) => dest.Themes = children
           );

            builder.AddChildAsync<TemplateItemInfo, TemplateFieldTypeViewModel, Guid>(
               items,
               parentKey: parent => parent.Id,
               childKey: child => child.TemplateId,
               assign: (dest, children) => dest.FieldTypes = children,
               async (src, dest) =>
               {
                   dest.Icon = await _moduleService.GetFieldTypeIconAsync(src.ItemName);
               });

            var result = await builder.BuildAsync(templates);
            return result;
        }
    }
}
