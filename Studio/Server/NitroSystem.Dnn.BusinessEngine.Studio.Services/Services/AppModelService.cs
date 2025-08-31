using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using System.Text.RegularExpressions;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration;
using DotNetNuke.Entities.Portals;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class AppModelService : IAppModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;
        private readonly IGlobalService _globalService;
        private readonly ITypeBuilderService _typeBuilderService;

        public AppModelService(IUnitOfWork unitOfWork, IRepositoryBase repository, IGlobalService globalService, ITypeBuilderService typeBuilderService)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _globalService = globalService;
            _typeBuilderService = typeBuilderService;
        }

        #region App Model 

        public async Task<AppModelViewModel> GetAppModelAsync(Guid appModuleId)
        {
            var appModel = await _repository.GetAsync<AppModelInfo>(appModuleId);
            var properties = await _repository.GetByScopeAsync<AppModelPropertyInfo>(appModuleId, "ViewOrder");

            return AppModelMapping.MapAppModelViewModel(appModel, properties);
        }

        public async Task<IEnumerable<AppModelViewModel>> GetAppModelsAsync(Guid scenarioId, string sortBy = "ViewOrder")
        {
            var appModels = await _repository.GetByScopeAsync<AppModelInfo>(scenarioId, sortBy);

            return AppModelMapping.MapAppModelsViewModel(appModels, Enumerable.Empty<AppModelPropertyInfo>());
        }

        public async Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string sortBy)
        {
            var results =
                await _repository
                    .ExecuteStoredProcedureMultiGridResultAsync(
                        "BusinessEngine_GetAppModelsWithProperties", "Studio_AppModels_",
                        new
                        {
                            ScenarioId = scenarioId,
                            SearchText = searchText,
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            SortBy = sortBy
                        },
                        grid => grid.ReadSingle<int?>(),
                        grid => grid.Read<AppModelInfo>(),
                        grid => grid.Read<AppModelPropertyInfo>()
                    );

            var totalCount = results[0] as int?;
            var appModels = results[1] as IEnumerable<AppModelInfo>;
            var appModelProperties = results[2] as IEnumerable<AppModelPropertyInfo>;

            return (AppModelMapping.MapAppModelsViewModel(appModels, appModelProperties), totalCount);
        }

        public async Task<Guid> SaveAppModelAsync(AppModelViewModel appModel, bool isNew, PortalSettings portalSettings)
        {
            var objAppModelInfo = AppModelMapping.MapAppModelInfo(appModel);

            _unitOfWork.BeginTransaction();

            try
            {
                var scenarioName = await _globalService.GetScenarioNameAsync(appModel.ScenarioId);
                var outputPath = $@"{portalSettings.HomeSystemDirectoryMapPath}\business-engine\{scenarioName}\AppModelTypes";
                var appModelDto = new AppModelDto()
                {
                    ModelName = appModel.ModelName,
                    ScenarioName = scenarioName,
                    Properties = appModel.Properties.Select(prop =>
                        HybridMapper.MapWithConfig<AppModelPropertyInfo, PropertyDefinition>(prop,
                            (src, dest) =>
                            {
                                dest.Name = prop.PropertyName;
                                dest.ClrType = prop.PropertyType;
                            }
                    )).ToList()
                };

                objAppModelInfo.TypeFullName = _typeBuilderService.BuildAppModelAsType(appModelDto, outputPath);
                objAppModelInfo.TypeRelativePath = $"{portalSettings.HomeSystemDirectory}/business-engine/{scenarioName}/AppModelTypes/";

                if (isNew)
                    objAppModelInfo.Id = await _repository.AddAsync<AppModelInfo>(objAppModelInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync<AppModelInfo>(objAppModelInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objAppModelInfo);
                }

                await _repository.DeleteByScopeAsync<AppModelPropertyInfo>(objAppModelInfo.Id);

                foreach (var objAppModelPropertyInfo in appModel.Properties)
                {
                    objAppModelPropertyInfo.AppModelId = objAppModelInfo.Id;

                    await _repository.AddAsync<AppModelPropertyInfo>(objAppModelPropertyInfo);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return objAppModelInfo.Id;
        }

        public async Task<bool> UpdateGroupColumn(Guid appModelId, Guid? groupId)
        {
            return await _repository.UpdateColumnAsync<AppModelInfo>("GroupId", groupId, appModelId);
        }

        public async Task<bool> DeleteAppModelAsync(Guid appModelId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var result = await _repository.DeleteAsync<AppModelInfo>(appModelId);

                _unitOfWork.Commit();

                return result;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }
        }

        #endregion
    }
}
