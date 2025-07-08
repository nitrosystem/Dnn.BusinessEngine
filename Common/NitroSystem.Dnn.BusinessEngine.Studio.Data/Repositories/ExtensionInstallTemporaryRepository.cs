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
    public class ExtensionInstallTemporaryRepository
    {
        public static ExtensionInstallTemporaryRepository Instance
        {
            get
            {
                return new ExtensionInstallTemporaryRepository();
            }
        }

        private const string CachePrefix = "BE_ExtensionInstallTemporaries_";

        public Guid AddItem(ExtensionInstallTemporaryView  objExpressionInfo)
        {
            objExpressionInfo.ItemID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionInstallTemporaryView>();
                rep.Insert(objExpressionInfo);

                DataCache.ClearCache(CachePrefix);

                return objExpressionInfo.ItemID;
            }
        }

        public void DeleteItem(Guid itemID)
        {
            ExtensionInstallTemporaryView objExpressionInfo = GetExpression(itemID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionInstallTemporaryView>();
                rep.Delete(objExpressionInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public ExtensionInstallTemporaryView GetExpression(Guid itemID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionInstallTemporaryView>();
                return rep.GetById(itemID);
            }
        }

        public IEnumerable<ExtensionInstallTemporaryView> GetExpressions(Guid typeID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<ExtensionInstallTemporaryView>();
                return rep.Get(typeID);
            }
        }
    }
}