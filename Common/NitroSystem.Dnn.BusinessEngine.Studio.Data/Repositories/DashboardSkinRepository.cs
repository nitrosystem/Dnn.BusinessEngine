using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class DashboardSkinRepository
    {
        public static DashboardSkinRepository Instance
        {
            get
            {
                return new DashboardSkinRepository();
            }
        }

        private const string CachePrefix = "BE_DashbaordSkins_";

        public Guid AddSkin(DashboardSkinInfo objSkinInfo)
        {
            objSkinInfo.SkinID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();
                rep.Insert(objSkinInfo);

                DataCache.ClearCache(CachePrefix);

                return objSkinInfo.SkinID;
            }
        }

        public void UpdateSkin(DashboardSkinInfo objSkinInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();
                rep.Update(objSkinInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteSkin(Guid databaseID)
        {
            DashboardSkinInfo objSkinInfo = GetSkin(databaseID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();
                rep.Delete(objSkinInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteSkinsByExtensionID(Guid extensionID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();
                rep.Delete("Where ExtensionID =@0", extensionID);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public DashboardSkinInfo GetSkin(Guid databaseID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();
                return rep.GetById(databaseID);
            }
        }

        public DashboardSkinInfo GetSkin(string skinName)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();
                var result = rep.Find("Where SkinName = @0", skinName);
                return result.Any() ? result.First() : null;
            }
        }

        public IEnumerable<DashboardSkinInfo> GetSkins()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<DashboardSkinInfo>();

                return rep.Get();
            }
        }
    }
}