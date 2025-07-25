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
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;

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
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Entity;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using DotNetNuke.Collections;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.Services
{
    public class DefinedListService : IDefinedListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IRepositoryBase _repository;

        public DefinedListService(IUnitOfWork unitOfWork, ICacheService cacheService, IRepositoryBase repository)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _repository = repository;
        }

        #region DefinedList Services

        //public async Task<IEnumerable<DefinedListViewModel>> GetDefinedListsViewModelAsync(ModuleType moduleType)
        //{
        //    var task1 = _repository.GetByScopeAsync<DefinedListInfo>(moduleType);
        //    var task2 = _repository.GetAllAsync<DefinedListThemeInfo>();
        //    var task3 = _repository.ExecuteStoredProcedureAsListAsync<ModuleFieldTypeDefinedListInfo>("BusinessEngine_GetDefinedListFieldTypes", null);

        //    await Task.WhenAll(task1, task2);

        //    var templates = await task1;
        //    var themes = await task2;
        //    var fieldTypes = await task3;

        //    return templates.Select(source =>
        //    {
        //        return HybridMapper.MapWithConfig<DefinedListInfo, DefinedListViewModel>(
        //            source, (src, dest) =>
        //            {
        //                dest.Themes = themes.Where(t => t.DefinedListId == source.Id).Select(theme =>
        //                {
        //                    theme.ThemeCssPath = theme.ThemeCssPath.ReplaceFrequentTokens();
        //                    return theme;
        //                });
        //                dest.DefinedListImage = dest.DefinedListImage.ReplaceFrequentTokens();
        //                dest.DefinedListPath = dest.DefinedListPath.ReplaceFrequentTokens();
        //                dest.PreviewImages = dest.PreviewImages.ReplaceFrequentTokens();
        //            });
        //    });
        //}

        public async Task<DefinedListViewModel> GetDefinedListByFieldId(Guid fieldId)
        {
            DefinedListViewModel definedList = null;

            var objDefinedListInfo = await _repository.GetByColumnAsync<DefinedListInfo>("FieldId", fieldId);
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

            var objDefinedListInfo = HybridMapper.MapWithChildCollection<DefinedListViewModel, DefinedListInfo, DefinedListItemViewModel, DefinedListItemInfo>(
                source: definedList,
                childSelector: vm => vm.Items,
                childAssigner: (info, columns) => items = columns
            );

            if (isNew)
                objDefinedListInfo.Id = await _repository.AddAsync<DefinedListInfo>(objDefinedListInfo);
            else
            {
                var isUpdated = await _repository.UpdateAsync<DefinedListInfo>(objDefinedListInfo);
                if (!isUpdated) ErrorService.ThrowUpdateFailedException(objDefinedListInfo);
            }

            if (definedList.Id != Guid.Empty) await _repository.DeleteAsync<DefinedListItemInfo>(definedList.Id);

            foreach (var objDefinedListItemInfo in items)
            {
                objDefinedListItemInfo.ListId = objDefinedListInfo.Id;

                if (objDefinedListItemInfo.Id == Guid.Empty)
                    await _repository.AddAsync<DefinedListItemInfo>(objDefinedListItemInfo);
                else
                {
                    var isUpdated = await _repository.UpdateAsync<DefinedListItemInfo>(objDefinedListItemInfo);
                    if (!isUpdated) ErrorService.ThrowUpdateFailedException(objDefinedListItemInfo);
                }
            }

            return objDefinedListInfo.Id;
        }

        #endregion
    }
}
