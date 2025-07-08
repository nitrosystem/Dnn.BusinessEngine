using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.DTO;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Controllers
{
    public static class GroupController
    {
        public static void ImportGroup(string json, PortalSettings portalSettings, UserInfo user, IDataContext ctx, HttpContext httpContext)
        {
            var groups = JsonConvert.DeserializeObject<IEnumerable<GroupInfo>>(json);
            foreach (var group in groups) SaveGroup(group, true, user, true, ctx);
        }

        public static GroupInfo SaveGroup(GroupInfo group, bool isNew, UserInfo user, bool calledFromImport, IDataContext ctx)
        {
            var objGroupInfo = new GroupInfo();
            group.CopyProperties(objGroupInfo);
            objGroupInfo.LastModifiedOnDate = group.LastModifiedOnDate = DateTime.Now;
            objGroupInfo.LastModifiedByUserID = group.LastModifiedByUserID = user.UserID;

            GroupRepository worker = calledFromImport ? new GroupRepository(ctx) : GroupRepository.Instance;

            if (isNew)
            {
                objGroupInfo.CreatedOnDate = group.CreatedOnDate = DateTime.Now;
                objGroupInfo.CreatedByUserID = group.CreatedByUserID = user.UserID;

                group.GroupID = worker.AddGroup(objGroupInfo);
            }
            else
            {
                objGroupInfo.CreatedOnDate = group.CreatedOnDate == DateTime.MinValue ? DateTime.Now : group.CreatedOnDate;
                objGroupInfo.CreatedByUserID = group.CreatedByUserID;

                worker.UpdateGroup(objGroupInfo);
            }

            return group;
        }

        public static void DeleteGroup(Guid groupID)
        {
            GroupRepository.Instance.DeleteGroup(groupID);
        }
    }
}
