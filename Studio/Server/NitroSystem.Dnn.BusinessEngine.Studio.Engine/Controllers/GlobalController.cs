using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.BusinessLogic.ViewModels;
using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Common.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.DatabaseService;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using NitroSystem.Dnn.BusinessEngine.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Data.RepositoryBase;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.BusinessLogic.Controllers
{
    public  class GlobalController
    {
        private readonly IUnitOfWork _unitOfWork;

        public GlobalController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public static void ImportScenario(string json, PortalSettings portalSettings, UserInfo user, HttpContext httpContext)
        //{
        //    var scenario = JsonConvert.DeserializeObject<ScenarioInfo>(json);
        //    SaveScenario(scenario, true, user, true, ctx);
        //}

        public void DeleteScenarioAndChilds(Guid scenarioID)
        {
            var relationships = GlobalRepository.Instance.GetRelationships();
            //var items = DbUtil.GetOrderedTables(relationships);
            //var mustBeDeleted = relationships.Where(r => r.ParentTable == BaseEntity.Scenario.TableName).Select(r => r.ChildTable);
            //var finalItems = items.Where(i => i == BaseEntity.Scenario.TableName || mustBeDeleted.Contains(i)).Reverse();

            //_repository.DeleteEntitiesRow<Guid>(finalItems, "ScenarioID", scenarioID);
            //_unitOfWork.Commit();
        }
    }
}
