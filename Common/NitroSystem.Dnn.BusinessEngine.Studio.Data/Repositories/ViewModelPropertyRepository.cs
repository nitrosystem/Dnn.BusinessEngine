using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ViewModelPropertyRepository
    {
        private readonly IDataContext _ctx;
        private const string _cachePrefix = "BE_ViewModelProperties_";

        public ViewModelPropertyRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static ViewModelPropertyRepository Instance
        {
            get
            {
                return new ViewModelPropertyRepository(DataContext.Instance());
            }
        }

        public Guid AddProperty(ViewModelPropertyInfo objViewModelPropertyInfo)
        {
            Guid propertyID = objViewModelPropertyInfo.PropertyID;
            objViewModelPropertyInfo.PropertyID = propertyID == Guid.Empty ? Guid.NewGuid() : objViewModelPropertyInfo.PropertyID;

            var rep = _ctx.GetRepository<ViewModelPropertyInfo>();
            rep.Insert(objViewModelPropertyInfo);

            DataCache.ClearCache(_cachePrefix);

            return objViewModelPropertyInfo.PropertyID;
        }

        public void UpdateProperty(ViewModelPropertyInfo objViewModelPropertyInfo)
        {
            var rep = _ctx.GetRepository<ViewModelPropertyInfo>();
            rep.Update(objViewModelPropertyInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public void DeleteProperty(Guid propertyID)
        {
            ViewModelPropertyInfo objViewModelPropertyInfo = GetProperty(propertyID);

            var rep = _ctx.GetRepository<ViewModelPropertyInfo>();
            rep.Delete(objViewModelPropertyInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public void DeleteProperties(IEnumerable<Guid> propertyIDs)
        {
            if (propertyIDs != null && propertyIDs.Any())
            {
                var rep = _ctx.GetRepository<ViewModelPropertyInfo>();
                rep.Delete("Where PropertyID in (@0)", string.Join(",", propertyIDs));

                DataCache.ClearCache(_cachePrefix);
            }
        }

        public ViewModelPropertyInfo GetProperty(Guid propertyID)
        {
            var rep = _ctx.GetRepository<ViewModelPropertyInfo>();
            return rep.GetById(propertyID);
        }

        public IEnumerable<ViewModelPropertyInfo> GetProperties(Guid viewModelID)
        {
            var rep = _ctx.GetRepository<ViewModelPropertyInfo>();
            return rep.Get(viewModelID).OrderBy(c => c.ViewOrder);
        }
    }
}