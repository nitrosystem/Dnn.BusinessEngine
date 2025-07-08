using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class VariableTypeRepository
    {
        public static VariableTypeRepository Instance
        {
            get
            {
                return new VariableTypeRepository();
            }
        }

        private const string CachePrefix = "BE_VariableTypes_";

        public Guid AddVariableType(VariableTypeInfo objVariableTypeInfo)
        {
            objVariableTypeInfo.TypeID = Guid.NewGuid();

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VariableTypeInfo>();
                rep.Insert(objVariableTypeInfo);

                DataCache.ClearCache(CachePrefix);

                return objVariableTypeInfo.TypeID;
            }
        }

        public void UpdateVariableType(VariableTypeInfo objVariableTypeInfo)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VariableTypeInfo>();
                rep.Update(objVariableTypeInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteVariableType(Guid typeID)
        {
            VariableTypeInfo objVariableTypeInfo = GetVariableType(typeID);
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VariableTypeInfo>();
                rep.Delete(objVariableTypeInfo);
            }

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteVariableTypes(IEnumerable<Guid> variablesID)
        {
            if (variablesID != null && variablesID.Any())
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var rep = ctx.GetRepository<VariableTypeInfo>();
                    rep.Delete("Where TypeID in (@0)", string.Join(",", variablesID));
                }

                DataCache.ClearCache(CachePrefix);
            }
        }

        public VariableTypeInfo GetVariableType(Guid typeID)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VariableTypeInfo>();
                return rep.GetById(typeID);
            }
        }

        public IEnumerable<VariableTypeInfo> GetVariableTypes()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<VariableTypeInfo>();

                return rep.Get().OrderBy(v=>v.ViewOrder).ThenBy(v=>v.TypeID);
            }
        }
    }
}