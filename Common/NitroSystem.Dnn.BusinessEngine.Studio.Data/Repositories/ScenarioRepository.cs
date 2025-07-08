using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Data.Repositories
{
    public class ScenarioRepository
    {
        private readonly IDataContext _ctx;
        private const string _cachePrefix = "BE_Scenarios_";

        public ScenarioRepository(IDataContext ctx)
        {
            _ctx = ctx;
        }

        public static ScenarioRepository Instance
        {
            get
            {
                return new ScenarioRepository(DataContext.Instance());
            }
        }


        public Guid AddScenario(ScenarioInfo objScenarioInfo)
        {
            Guid scenarioID = objScenarioInfo.ScenarioID;
            objScenarioInfo.ScenarioID = scenarioID == Guid.Empty ? Guid.NewGuid() : objScenarioInfo.ScenarioID;

            var rep = _ctx.GetRepository<ScenarioInfo>();
            rep.Insert(objScenarioInfo);

            DataCache.ClearCache(_cachePrefix);

            return objScenarioInfo.ScenarioID;
        }

        public void UpdateScenario(ScenarioInfo objScenarioInfo)
        {
            var rep = _ctx.GetRepository<ScenarioInfo>();
            rep.Update(objScenarioInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public void DeleteScenario(Guid scenarioID)
        {
            ScenarioInfo objScenarioInfo = GetScenario(scenarioID);
            var rep = _ctx.GetRepository<ScenarioInfo>();
            rep.Delete(objScenarioInfo);

            DataCache.ClearCache(_cachePrefix);
        }

        public ScenarioInfo GetScenario(Guid scenarioID)
        {
            var rep = _ctx.GetRepository<ScenarioInfo>();
            return rep.GetById(scenarioID);
        }

        public ScenarioInfo GetScenario(string scenarioName)
        {
            return GetScenarios().FirstOrDefault(s => s.ScenarioName == scenarioName);
        }

        public string GetScenarioName(Guid scenarioID)
        {
            var rep = _ctx.GetRepository<ScenarioInfo>();
            return rep.GetById(scenarioID).ScenarioName;
        }

        public IEnumerable<ScenarioInfo> GetScenarios()
        {
            var rep = _ctx.GetRepository<ScenarioInfo>();
            return rep.Get();
        }
    }
}