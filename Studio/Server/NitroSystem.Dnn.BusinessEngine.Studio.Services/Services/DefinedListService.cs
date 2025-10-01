using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.UnitOfWork;
using NitroSystem.Dnn.BusinessEngine.Core.Security;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Base;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class DefinedListService : IDefinedListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryBase _repository;

        public DefinedListService(IUnitOfWork unitOfWork, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<DefinedListViewModel> GetDefinedListByListName(string listName)
        {
            DefinedListViewModel definedList = null;

            var objDefinedListInfo = await _repository.GetByColumnAsync<DefinedListInfo>("ListName", listName);
            if (objDefinedListInfo != null)
            {
                var items = await _repository.GetByScopeAsync<DefinedListItemInfo>(objDefinedListInfo.Id, "ViewOrder");

                definedList = HybridMapper.MapWithConfig<DefinedListInfo, DefinedListViewModel>(
                   objDefinedListInfo, (src, dest) =>
                   {
                       dest.Items = items.Select(item =>
                       {
                           return HybridMapper.Map<DefinedListItemInfo, DefinedListItemViewModel>(item);
                       });
                   });
            }

            return definedList;
        }

        public async Task<Guid> SaveDefinedList(DefinedListViewModel definedList, bool isNew)
        {
            IEnumerable<DefinedListItemInfo> items = Enumerable.Empty<DefinedListItemInfo>();

            var objDefinedListInfo = HybridMapper.MapWithChildren<DefinedListViewModel, DefinedListInfo, DefinedListItemViewModel, DefinedListItemInfo>(
                source: definedList,
                children: definedList.Items,
                assignChildren: (info, columns) => items = columns
            );

            _unitOfWork.BeginTransaction();

            try
            {
                if (isNew)
                    objDefinedListInfo.Id = await _repository.AddAsync(objDefinedListInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync(objDefinedListInfo);
                    if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDefinedListInfo);
                }

                if (definedList.Id != Guid.Empty) await _repository.DeleteAsync<DefinedListItemInfo>(definedList.Id);

                foreach (var objDefinedListItemInfo in items)
                {
                    objDefinedListItemInfo.ListId = objDefinedListInfo.Id;

                    if (objDefinedListItemInfo.Id == Guid.Empty)
                        await _repository.AddAsync(objDefinedListItemInfo);
                    else
                    {
                        var isUpdated = await _repository.UpdateAsync(objDefinedListItemInfo);
                        if (!isUpdated) ErrorHandling.ThrowUpdateFailedException(objDefinedListItemInfo);
                    }
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                throw ex;
            }

            return objDefinedListInfo.Id;
        }
    }
}
