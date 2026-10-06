using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DefinedList;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Studio.ApplicationService.DefinedList
{
    public class DefinedListService : IDefinedListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDefinedListRepository _definedListRepository;
        private readonly IDefinedListItemRepository _definedListItemRepository;

        public DefinedListService(IUnitOfWork unitOfWork, IDefinedListRepository definedListRepository, IDefinedListItemRepository definedListItemRepository)
        {
            _unitOfWork = unitOfWork;
            _definedListRepository = definedListRepository;
            _definedListItemRepository = definedListItemRepository;
        }

        public async Task<IEnumerable<DefinedListViewModel>> GetDefinedListsViewModelAsync(Guid scenarioId)
        {
            var definedLists = await _definedListRepository.GetsAsync(scenarioId, "ListName");
            var definedListsItems = await _definedListItemRepository.GetsByAncestorAsync(scenarioId);

            return HybridMapper.MapWithChildren<DefinedListInfo, DefinedListViewModel, DefinedListItemInfo, DefinedListItemViewModel>(
              parents: definedLists,
              children: definedListsItems,
              parentKeySelector: p => p.Id,
              childKeySelector: c => c.ListId,
              assignChildren: (parent, childs) => parent.Items = childs
            );
        }

        public async Task<DefinedListViewModel> GetDefinedListViewModelByListNameAsync(string listName, string sortBy = "ViewOrder")
        {
            var objDefinedListInfo = await _definedListRepository.GetAsync(listName);
            if (objDefinedListInfo != null)
            {
                var items = await _definedListItemRepository.GetsAsync(objDefinedListInfo.Id, sortBy);
                return HybridMapper.MapWithChildren<DefinedListInfo, DefinedListViewModel, DefinedListItemInfo, DefinedListItemViewModel>(
                    source: objDefinedListInfo,
                    children: items,
                    assignChildren: (parent, childs) => parent.Items = childs);
            }

            return null;
        }

        public async Task<Guid> CreateDefinedListAsync(DefinedListViewModel definedList)
        {
            var objDefinedListInfo = HybridMapper.Map<DefinedListViewModel, DefinedListInfo>(definedList);
            var listItems = HybridMapper.MapCollection<DefinedListItemViewModel, DefinedListItemInfo>(definedList.Items);

            _unitOfWork.BeginTransaction();

            try
            {
                if (objDefinedListInfo.Id == Guid.Empty)
                {
                    objDefinedListInfo.Id = await _definedListRepository.AddAsync(objDefinedListInfo);
                }
                else
                {
                    await _definedListRepository.UpdateAsync(objDefinedListInfo);

                    //Delete old DefinedListItems
                    await _definedListItemRepository.DeletesAsync(definedList.Id);
                }

                //Add new DefinedListItems
                await _definedListItemRepository.BulkInsertAsync(listItems.Select(i => { i.ListId = objDefinedListInfo.Id; return i; }));

                _unitOfWork.Commit();

                return objDefinedListInfo.Id;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }
    }
}
