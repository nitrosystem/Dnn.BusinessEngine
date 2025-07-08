using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Data.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ExtensionExportItemRepository
    {
        public static ExtensionExportItemRepository Instance
        {
            get
            {
                return new ExtensionExportItemRepository();
            }
        }

        private const string CachePrefix = "BE_ExtensionExportItems_";

        public Guid AddExportItem(ExtensionImportExportItemInfo objExtensionExportItemInfo)
        {
            objExtensionExportItemInfo.ItemID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();
                rep.Insert(objExtensionExportItemInfo);

                DataCache.ClearCache(CachePrefix);

                return objExtensionExportItemInfo.ItemID;
            }
        }

        public void UpdateExportItem(ExtensionImportExportItemInfo objExtensionExportItemInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();
                rep.Update(objExtensionExportItemInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteExportItem(Guid itemID)
        {
            ExtensionImportExportItemInfo objExtensionExportItemInfo = GetExportItem(itemID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();
                rep.Delete(objExtensionExportItemInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public ExtensionImportExportItemInfo GetExportItem(Guid itemID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();
                return rep.GetById(itemID);
            }
        }

        public IEnumerable<ExtensionImportExportItemInfo> GetExportItems(string extensionName)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();

                return rep.Find("WHERE ExtensionName=@0", extensionName);
            }
        }

        public IEnumerable<ExtensionImportExportItemInfo> GetExportItemsByType(OperationType operationType, string itemType)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();

                return rep.Find("WHERE OperationType=@0 and itemType=@1", operationType, itemType);
            }
        }

        public IEnumerable<ExtensionImportExportItemInfo> GetExportItems()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionImportExportItemInfo>();

                return rep.Get();
            }
        }
    }
}