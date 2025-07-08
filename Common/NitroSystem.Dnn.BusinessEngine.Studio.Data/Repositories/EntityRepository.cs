using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Common.Models;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class EntityRepository
    {
        private readonly IDataContext _ctx;
        private const string _cachePrefix = "BE_Entities_";

        public EntityRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static EntityRepository Instance
        {
            get
            {
                return new EntityRepository(DataContext.Instance());
            }
        }

        public Guid AddEntity(EntityInfo objEntityInfo)
        {
            Guid entityID = objEntityInfo.EntityID;
            objEntityInfo.EntityID = entityID == Guid.Empty ? Guid.NewGuid() : objEntityInfo.EntityID;

            var rep = _ctx.GetRepository<EntityInfo>();
            rep.Insert(objEntityInfo);

            DataCache.ClearCache(_cachePrefix);

            return objEntityInfo.EntityID;
        }

        public void UpdateEntity(EntityInfo objEntityInfo)
        {
            var rep = _ctx.GetRepository<EntityInfo>();
            rep.Update(objEntityInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public void UpdateGroup(Guid itemID, Guid? groupID)
        {
            _ctx.Execute(System.Data.CommandType.Text, "UPDATE dbo.BusinessEngine_Entities SET GroupID = @0 WHERE EntityID = @1", groupID, itemID);

            DataCache.ClearCache(_cachePrefix);
        }

        public void DeleteEntity(Guid entityID)
        {
            EntityInfo objEntityInfo = GetEntity(entityID);
            var rep = _ctx.GetRepository<EntityInfo>();
            rep.Delete(objEntityInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public EntityInfo GetEntity(Guid entityID)
        {
            var rep = _ctx.GetRepository<EntityInfo>();
            return rep.GetById(entityID);
        }

        public EntityInfo GetEntityByModuleID(Guid entityID)
        {
            var rep = _ctx.GetRepository<EntityInfo>();
            return rep.GetById(entityID);
        }

        public IEnumerable<EntityInfo> GetEntities(Guid scenarioID)
        {
            var rep = _ctx.GetRepository<EntityInfo>();

            return rep.Get(scenarioID).OrderBy(e => e.ViewOrder);
        }

        public IPagedList<EntityInfo> GetEntities(Guid scenarioID, int pageIndex, int pageSize, string searchText, string entityType)
        {
            if (searchText == null) searchText = "";

            var rep = _ctx.GetRepository<EntityInfo>();
            return rep.Find(pageIndex, pageSize, "WHERE ScenarioID = @0 and EntityName like N'%' + @1 + '%' and (ISNULL(@2,'') = '' or EntityType =  @2) ORDER BY ViewOrder", scenarioID, searchText, entityType);
        }

        public string[] GetTableEntities(Guid scenarioID, bool isReadonly)
        {
            var rep = _ctx.GetRepository<EntityInfo>();
            var result = rep.Get(scenarioID).Where(e => e.EntityType == "table" && e.IsReadonly == isReadonly).Select(e => e.TableName).ToArray();
            return result;
        }

        public IEnumerable<EntityInfo> GetAllEntities()
        {
            var rep = _ctx.GetRepository<EntityInfo>();
            return rep.Get().OrderBy(e => e.ViewOrder);
        }
    }
}