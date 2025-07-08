using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ViewModelRepository
    {
        private readonly IDataContext _ctx;
        private const string _cachePrefix = "BE_ViewModels_";

        public ViewModelRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static ViewModelRepository Instance
        {
            get
            {
                return new ViewModelRepository(DataContext.Instance());
            }
        }

        public Guid AddViewModel(ViewModelInfo objViewModelInfo)
        {
            Guid viewModelID = objViewModelInfo.ViewModelID;
            objViewModelInfo.ViewModelID = viewModelID == Guid.Empty ? Guid.NewGuid() : objViewModelInfo.ViewModelID;

            var rep = _ctx.GetRepository<ViewModelInfo>();
            rep.Insert(objViewModelInfo);

            DataCache.ClearCache(_cachePrefix);

            return objViewModelInfo.ViewModelID;
        }

        public void UpdateViewModel(ViewModelInfo objViewModelInfo)
        {
            var rep = _ctx.GetRepository<ViewModelInfo>();
            rep.Update(objViewModelInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public void UpdateGroup(Guid itemID, Guid? groupID)
        {
            _ctx.Execute(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_ViewModels SET GroupID = @0 WHERE ViewModelID = @1", groupID, itemID);

            DataCache.ClearCache(_cachePrefix);
        }

        public void DeleteViewModel(Guid viewModelID)
        {
            ViewModelInfo objViewModelInfo = GetViewModel(viewModelID);

            var rep = _ctx.GetRepository<ViewModelInfo>();
            rep.Delete(objViewModelInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public ViewModelInfo GetViewModel(Guid viewModelID)
        {
            var rep = _ctx.GetRepository<ViewModelInfo>();
            return rep.GetById(viewModelID);
        }

        public IPagedList<ViewModelInfo> GetViewModels(Guid scenarioID, int pageIndex, int pageSize, string searchText)
        {
            if (searchText == null) searchText = "";

            var rep = _ctx.GetRepository<ViewModelInfo>();
            return rep.Find(pageIndex, pageSize, "WHERE ScenarioID = @0 and ViewModelName like N'%' + @1 + '%' ORDER BY ViewOrder", scenarioID, searchText);
        }

        public IEnumerable<ViewModelInfo> GetViewModels(Guid scenarioID)
        {
            var rep = _ctx.GetRepository<ViewModelInfo>();
            return rep.Get(scenarioID).OrderBy(v => v.ViewOrder);
        }
    }
}