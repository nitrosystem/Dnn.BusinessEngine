using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Tokens;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using System.Runtime.Remoting.Messaging;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public TemplateService(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = new RepositoryBase(_unitOfWork, _cacheService);
        }

        #region Template Services

        public async Task<IEnumerable<TemplateViewModel>> GetTemplatesViewModelAsync(ModuleType moduleType)
        {
            var task1 = _repository.GetByScopeAsync<TemplateInfo>(moduleType);
            var task2 = _repository.GetAllAsync<TemplateThemeInfo>();
            var task3 = _repository.ExecuteStoredProcedureAsListAsync<ModuleFieldTypeTemplateInfo>("BusinessEngine_GetTemplateFieldTypes", null);

            await Task.WhenAll(task1, task2);

            var templates = await task1;
            var themes = await task2;
            var fieldTypes = await task3;

            return templates.Select(source =>
            {
                return HybridMapper.MapWithConfig<TemplateInfo, TemplateViewModel>(
                    source, (src, dest) =>
                    {
                        dest.Themes = themes.Where(t => t.TemplateId == source.Id).Select(theme =>
                        {
                            theme.ThemeCssPath = theme.ThemeCssPath.ReplaceFrequentTokens();
                            return theme;
                        });
                        dest.TemplateImage = dest.TemplateImage.ReplaceFrequentTokens();
                        dest.TemplatePath = dest.TemplatePath.ReplaceFrequentTokens();
                        dest.PreviewImages = dest.PreviewImages.ReplaceFrequentTokens();
                    });
            });
        }

        public async Task<TemplateViewModel> GetTemplateViewModelAsync(Guid id)
        {
            return null;
        }

        #endregion
    }
}
