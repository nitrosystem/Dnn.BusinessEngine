using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class GroupRepository
    {
        private readonly IDataContext _ctx;
        private const string _cachePrefix = "BE_Groups_";

        public GroupRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static GroupRepository Instance
        {
            get
            {
                return new GroupRepository(DataContext.Instance());
            }
        }

        public Guid AddGroup(GroupInfo objGroupInfo)
        {
            Guid groupID = objGroupInfo.GroupID;
            objGroupInfo.GroupID = groupID == Guid.Empty ? Guid.NewGuid() : objGroupInfo.GroupID;

            var rep = _ctx.GetRepository<GroupInfo>();
            rep.Insert(objGroupInfo);

            DataCache.ClearCache(_cachePrefix);

            return objGroupInfo.GroupID;
        }

        public Guid CheckExistsGroupOrCreateGroup(Guid scenarioID, int userID, string groupType, string groupName)
        {
            var group = GetGroupByName(scenarioID, groupType, groupName);

            if (group == null)
            {
                var objGroupInfo = new GroupInfo()
                {
                    GroupID = Guid.NewGuid(),
                    ScenarioID = scenarioID,
                    GroupType = groupType,
                    GroupName = groupName,
                    CreatedOnDate = DateTime.Now,
                    LastModifiedOnDate = DateTime.Now,
                    CreatedByUserID = userID,
                    LastModifiedByUserID = userID,
                };

                var rep = _ctx.GetRepository<GroupInfo>();
                rep.Insert(objGroupInfo);

                DataCache.ClearCache(_cachePrefix);

                return objGroupInfo.GroupID;
            }
            else
                return group.GroupID;
        }

        public void UpdateGroup(GroupInfo objGroupInfo)
        {
            var rep = _ctx.GetRepository<GroupInfo>();
            rep.Update(objGroupInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public void DeleteGroup(Guid groupID)
        {
            GroupInfo objGroupInfo = GetGroup(groupID);
            var rep = _ctx.GetRepository<GroupInfo>();
            rep.Delete(objGroupInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public GroupInfo GetGroup(Guid groupID)
        {
            var rep = _ctx.GetRepository<GroupInfo>();
            return rep.GetById(groupID);
        }

        public Guid? GetGroupID(string groupType, string groupName)
        {
            var rep = _ctx.GetRepository<GroupInfo>();
            var result = rep.Find("Where GroupType =@0 and GroupName =@1", groupType, groupName);
            if (result.Any())
                return result.First().GroupID;
            else return null;
        }

        public GroupInfo GetGroupByName(Guid scenarioID, string groupType, string groupName)
        {
            var rep = _ctx.GetRepository<GroupInfo>();
            var result = rep.Find("Where ScenarioID =@0 and GroupType =@1 and GroupName =@2", scenarioID, groupType, groupName);
            return result.Any() ? result.First() : null;
        }

        public IEnumerable<GroupInfo> GetGroups(Guid scenarioID)
        {
            var rep = _ctx.GetRepository<GroupInfo>();
            return rep.Get(scenarioID);
        }

        public IEnumerable<GroupInfo> GetGroups()
        {
            var rep = _ctx.GetRepository<GroupInfo>();
            return rep.Get();
        }
    }
}