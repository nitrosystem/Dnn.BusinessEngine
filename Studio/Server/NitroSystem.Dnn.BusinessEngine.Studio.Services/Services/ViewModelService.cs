using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Services;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.General;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
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

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class ViewModelService : IViewModelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public ViewModelService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
        }

        #region View Model 

        public async Task<ViewModelViewModel> GetViewModelAsync(Guid id)
        {
            var viewModel = await _repository.GetAsync<ViewModelInfo>(id);
            var properties = await _repository.GetByScopeAsync<ViewModelPropertyInfo>(id);

            return ViewModelMapping.MapViewModel(viewModel, properties);
        }

        public async Task<IEnumerable<ViewModelViewModel>> GetViewModelsAsync(Guid scenarioId, string sortBy = "ViewOrder")
        {
            var viewModels = await _repository.GetByScopeAsync<ViewModelInfo>(scenarioId, sortBy);

            return ViewModelMapping.MapViewModels(viewModels, Enumerable.Empty<ViewModelPropertyInfo>());
        }

        public async Task<(IEnumerable<ViewModelViewModel> Items, int? TotalCount)> GetViewModelsAsync(Guid scenarioId, int pageIndex, int pageSize, string searchText, string sortBy)
        {
            var results =
                await _repository
                    .ExecuteStoredProcedureMultiGridResultAsync(
                        "BusinessEngine_GetViewModelsWithProperties",
                        new
                        {
                            ScenarioId = scenarioId,
                            SearchText = searchText,
                            PageIndex = pageIndex,
                            PageSize = pageSize,
                            SortBy = sortBy
                        },
                        grid => grid.ReadSingle<int?>(),
                        grid => grid.Read<ViewModelInfo>(),
                        grid => grid.Read<ViewModelPropertyInfo>()
                    );

            var totalCount = results[0] as int?;
            var viewModels = results[1] as IEnumerable<ViewModelInfo>;
            var viewModelProperties = results[2] as IEnumerable<ViewModelPropertyInfo>;

            return (ViewModelMapping.MapViewModels(viewModels, viewModelProperties), totalCount);
        }

        public async Task<Guid> SaveViewModelAsync(ViewModelViewModel viewModel, bool isNew)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                var objViewModelInfo = ViewModelMapping.MapViewModelInfo(viewModel);

                if (isNew)
                    objViewModelInfo.Id = viewModel.Id = await _repository.AddAsync<ViewModelInfo>(objViewModelInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync<ViewModelInfo>(objViewModelInfo);
                    if (!isUpdated) ErrorService.ThrowUpdateFailedException(objViewModelInfo);
                }

                await _repository.DeleteByScopeAsync<ViewModelPropertyInfo>(objViewModelInfo.Id);

                foreach (var objViewModelPropertyInfo in viewModel.Properties)
                {
                    objViewModelPropertyInfo.ViewModelId = objViewModelInfo.Id;

                    if (objViewModelPropertyInfo.PropertyType != "viewModel" && objViewModelPropertyInfo.PropertyType != "listOfViewModel") objViewModelPropertyInfo.PropertyTypeId = null;

                    await _repository.AddAsync<ViewModelPropertyInfo>(objViewModelPropertyInfo);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return viewModel.Id;
        }

        public async Task<bool> UpdateGroupColumn(Guid viewModelId, Guid? groupId)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                await _repository.UpdateColumnAsync<ViewModelInfo>("GroupId", groupId, viewModelId);

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return true;
        }

        public async Task<bool> DeleteViewModelAsync(Guid id)
        {
            return await _repository.DeleteAsync<ViewModelInfo>(id);
        }

        #endregion
    }
}
