using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class EntityColumnRepository
    {
        private readonly IDataContext _ctx;

        public EntityColumnRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static EntityColumnRepository Instance
        {
            get
            {
                return new EntityColumnRepository(DataContext.Instance());
            }
        }

        private const string CachePrefix = "BE_EntityColumns_";

        public Guid AddColumn(EntityColumnInfo objEntityColumnInfo)
        {
            Guid columnID = objEntityColumnInfo.ColumnID;
            objEntityColumnInfo.ColumnID = columnID == Guid.Empty ? Guid.NewGuid() : objEntityColumnInfo.ColumnID;

            var rep = _ctx.GetRepository<EntityColumnInfo>();
            rep.Insert(objEntityColumnInfo);

            DataCache.ClearCache(CachePrefix);

            return objEntityColumnInfo.ColumnID;
        }

        public void UpdateColumn(EntityColumnInfo objEntityColumnInfo)
        {
            var rep = _ctx.GetRepository<EntityColumnInfo>();
            rep.Update(objEntityColumnInfo);

            DataCache.ClearCache(CachePrefix);
        }

        public void DeleteColumn(Guid entityColumnID)
        {
            EntityColumnInfo objEntityColumnInfo = GetColumn(entityColumnID);
            var rep = _ctx.GetRepository<EntityColumnInfo>();
            rep.Delete(objEntityColumnInfo);

            DataCache.ClearCache(CachePrefix);
        }

        public EntityColumnInfo GetColumn(Guid entityColumnID)
        {
            var rep = _ctx.GetRepository<EntityColumnInfo>();
            return rep.GetById(entityColumnID);
        }

        public IEnumerable<EntityColumnInfo> GetColumns(Guid entityID)
        {
            var rep = _ctx.GetRepository<EntityColumnInfo>();
            return rep.Get(entityID).OrderBy(c => c.ViewOrder);
        }
    }
}