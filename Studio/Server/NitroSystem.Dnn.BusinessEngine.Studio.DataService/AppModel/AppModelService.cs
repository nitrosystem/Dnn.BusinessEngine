using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.AppModel;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.AppModel
{
    public class AppModelService : IAppModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public AppModelService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<AppModelViewModel> GetAppModelAsync(Guid appModuleId)
        {
            var appModel = await _repository.GetAsync<AppModelInfo>(appModuleId);
            var properties = await _repository.GetByScopeAsync<AppModelPropertyInfo>(appModuleId, "ViewOrder");

            return HybridMapper.MapWithChildren<AppModelInfo, AppModelViewModel, AppModelPropertyInfo, AppModelPropertyViewModel>(
                source: appModel,
                children: properties,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<IEnumerable<AppModelViewModel>> GetAppModelsAsync(Guid scenarioId, string sortBy = "ViewOrder")
        {
            var appModels = await _repository.GetByScopeAsync<AppModelInfo>(scenarioId, sortBy);
            var properties = await _repository.GetAllAsync<AppModelPropertyInfo>("ViewOrder");

            return HybridMapper.MapWithChildren<AppModelInfo, AppModelViewModel,
                                                AppModelPropertyInfo, AppModelPropertyViewModel>(
                appModels,
                properties,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );
        }

        public async Task<(IEnumerable<AppModelViewModel> Items, int? TotalCount)> GetAppModelsAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string sortBy)
        {
            var results = await _repository.ExecuteStoredProcedureMultipleAsync<int?, AppModelInfo, AppModelPropertyInfo>(
                    "dbo.BusinessEngine_Studio_GetAppModelsWithProperties", "BE_AppModels_Studio_GetAppModelsWithProperties_",
                    new
                    {
                        ScenarioId = scenarioId,
                        SearchText = searchText,
                        PageIndex = pageIndex,
                        PageSize = pageSize,
                        SortBy = sortBy
                    }
                );

            var totalCount = results.Item1?.First();
            var appModels = results.Item2;
            var properties = results.Item3;

            var result = HybridMapper.MapWithChildren<AppModelInfo, AppModelViewModel,
                                                AppModelPropertyInfo, AppModelPropertyViewModel>(
                appModels,
                properties,
                parentKeySelector: p => p.Id,
                childKeySelector: c => c.AppModelId,
                assignChildren: (parent, childs) => parent.Properties = childs
            );

            return (result, totalCount);
        }

        public async Task<Guid> SaveAppModelAsync(AppModelViewModel appModel, bool isNew)
        {
            var objAppModelInfo = HybridMapper.Map<AppModelViewModel, AppModelInfo>(appModel);

            _unitOfWork.BeginTransaction();

            try
            {
                if (isNew)
                    objAppModelInfo.Id = await _repository.AddAsync<AppModelInfo>(objAppModelInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync<AppModelInfo>(objAppModelInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objAppModelInfo);
                }

                await _repository.DeleteByScopeAsync<AppModelPropertyInfo>(objAppModelInfo.Id);

                foreach (var prop in appModel.Properties)
                {
                    var objAppModelPropertyInfo = HybridMapper.Map<AppModelPropertyViewModel, AppModelPropertyInfo>(prop);
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

        public async Task<bool> UpdateGroupColumnAsync(Guid appModelId, Guid? groupId)
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
    }
}
