using DotNetNuke.Data;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildType.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Export;
using NitroSystem.Dnn.BusinessEngine.Core.ImportExport.Import;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.AppModel
{
    public class AppModelService : IAppModelService, IExportable, IImportable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBuildTypeRunner _buildTypeRunner;
        private readonly IAppModelRepository _appModelRepository;
        private readonly IAppModelPropertyRepository _appModelPropertyRepository;
        private readonly IScenarioRepository _scenarioRepository;

        public AppModelService(
            IUnitOfWork unitOfWork,
            IBuildTypeRunner buildTypeRunner,
            IAppModelRepository appModelRepository,
            IAppModelPropertyRepository appModelPropertyRepository,
            IScenarioRepository scenarioRepository)
        {
            _unitOfWork = unitOfWork;
            _buildTypeRunner = buildTypeRunner;
            _appModelRepository = appModelRepository;
            _appModelPropertyRepository = appModelPropertyRepository;
            _scenarioRepository = scenarioRepository;
        }

        public async Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsViewModelAsync(
            Guid scenarioId,
            int pageIndex,
            int pageSize,
            string searchText,
            AppModelType? modelType,
            string sortBy)
        {
            var results = await _appModelRepository.GetsAsync(scenarioId, pageIndex, pageSize, searchText, modelType, sortBy);
            var result = HybridMapper.MapWithChildren<AppModelInfo, AppModelViewModel,
                                                AppModelPropertyInfo, AppModelPropertyViewModel>(
                results.Models,
                results.Properties,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );

            return (result, results.TotalCount);
        }

        public async Task<IEnumerable<AppModelListItem>> GetAppModelsListItemAsync(Guid scenarioId, string sortBy)
        {
            var appModels = await _appModelRepository.GetsAsync(scenarioId, sortBy);
            var properties = await _appModelPropertyRepository.GetsAsync("ViewOrder");

            return HybridMapper.MapWithChildren<AppModelInfo, AppModelListItem, AppModelPropertyInfo, AppModelPropertyListItem>(
                appModels,
                properties,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<AppModelViewModel> GetAppModelViewModelAsync(Guid appModuleId)
        {
            var appModel = await _appModelRepository.GetAsync(appModuleId);
            var properties = await _appModelPropertyRepository.GetsAsync(appModuleId, "ViewOrder");

            return HybridMapper.MapWithChildren<AppModelInfo, AppModelViewModel, AppModelPropertyInfo, AppModelPropertyViewModel>(
                source: appModel,
                children: properties,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<Guid> CreateAppModelAsync(AppModelViewModel appModel, string relativePath, string basePath)
        {
            var scenarioName = await _scenarioRepository.GetScenarioNameAsync(appModel.ScenarioId);
            var properties = HybridMapper.MapCollection<AppModelPropertyViewModel, PropertyDefinition>(appModel.Properties);
            var request = new BuildTypeRequest()
            {
                ScenarioName = scenarioName,
                RelativePath = relativePath,
                BasePath = basePath,
                ModelName = appModel.ModelName,
                Version = "01.00.00",
                Properties = properties.Cast<IPropertyDefinition>().ToList()
            };
            var type = await _buildTypeRunner.RunAsync(request);
            appModel.TypeRelativePath = type.RelativePath;
            appModel.TypeFullName = type.TypeFullName;

            var objAppModelInfo = HybridMapper.Map<AppModelViewModel, AppModelInfo>(appModel);
            var modelProperties = HybridMapper.MapCollection<AppModelPropertyViewModel, AppModelPropertyInfo>(appModel.Properties);

            _unitOfWork.BeginTransaction();

            try
            {
                if (objAppModelInfo.Id == Guid.Empty)
                {
                    objAppModelInfo.Id = await _appModelRepository.AddAsync(objAppModelInfo);
                }
                else
                {
                    await _appModelRepository.UpdateAsync(objAppModelInfo);

                    //Delete old AppModelProperties
                    await _appModelPropertyRepository.DeletesAsync(objAppModelInfo.Id);
                }

                //Add new ActionParams
                await _appModelPropertyRepository.BulkInsertAsync(modelProperties.Select(m => { m.AppModelId = objAppModelInfo.Id; return m; }).ToList());

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }

            return objAppModelInfo.Id;
        }

        public async Task UpdateAppMopdelGroupAsync(Guid? groupId, Guid appModelId)
        {
            await _appModelRepository.UpdateGroupAsync(groupId, appModelId);
        }

        public async Task<bool> DeleteAppModelAsync(Guid appModelId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var result = await _appModelRepository.DeleteAsync(appModelId);
                _unitOfWork.Commit();
                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        #region Import Export

        public async Task<ExportResponse> ExportAsync(ExportContext context)
        {
            switch (context.Scope)
            {
                case ImportExportScope.ScenarioFullComponents:
                    var items = await GetModelsAndPropertiesAsync(context.Get<Guid>("ScenarioId"));

                    return new ExportResponse()
                    {
                        Result = items,
                        IsSuccess = true
                    };
                default:
                    return null;
            }
        }

        public async Task<ImportResponse> ImportAsync(string json, ImportContext context)
        {
            var items = JsonConvert.DeserializeObject<List<object>>(json);
            var models = JsonConvert.DeserializeObject<IReadOnlyList<AppModelInfo>>(items[0].ToString());
            var modelsProperties = JsonConvert.DeserializeObject<IReadOnlyList<AppModelPropertyInfo>>(items[1].ToString()).ToList();

            if (context.Scope == ImportExportScope.ScenarioFullComponents)
            {
                await BulkInsertAppModelsAndPropertiesAsync(models, modelsProperties);

                var basePath = context.Get<string>("BasePath");
                var scenarioName = context.Get<string>("ScenarioName");

                foreach (var model in models)
                {
                    var props = modelsProperties.Where(p => p.AppModelId == model.Id);
                    var properties = HybridMapper.MapCollection<AppModelPropertyInfo, PropertyDefinition>(props);
                    var request = new BuildTypeRequest()
                    {
                        ScenarioName = scenarioName,
                        BasePath = basePath,
                        ModelName = model.ModelName,
                        Version = "01.00.00",
                        Properties = properties.Cast<IPropertyDefinition>().ToList()
                    };

                    await _buildTypeRunner.RunAsync(request);
                }
            }

            return new ImportResponse()
            {
                IsSuccess = true
            };
        }

        private async Task<object> GetModelsAndPropertiesAsync(Guid scenarioId)
        {
            var models = await _appModelRepository.GetsAsync(scenarioId);
            var modelsProperties = new List<AppModelPropertyInfo>();

            foreach (var model in models)
            {
                modelsProperties.AddRange(await _appModelPropertyRepository.GetsAsync(model.Id));
            }

            return new List<object>() { models, modelsProperties };
        }

        private async Task BulkInsertAppModelsAndPropertiesAsync(IReadOnlyList<AppModelInfo> models, IReadOnlyList<AppModelPropertyInfo> modelsProperties)
        {
            await _appModelRepository.BulkInsertAsync(models);
            await _appModelPropertyRepository.BulkInsertAsync(modelsProperties);
        }

        #endregion
    }
}
