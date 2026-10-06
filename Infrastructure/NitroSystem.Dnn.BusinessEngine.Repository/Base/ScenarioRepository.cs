using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Base;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Base
{
    public class ScenarioRepository : IScenarioRepository
    {
        private readonly ISql _sql;

        public ScenarioRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<Guid> AddAsync(ScenarioInfo objScenarioInfo)
        {
            return await _sql.InsertAsync<ScenarioInfo>(objScenarioInfo);
        }

        public async Task<string> GetScenarioNameAsync(Guid id)
        {
            return await _sql.GetColumnValueAsync<ScenarioInfo, string>(id, "ScenarioName");
        }

        public async Task<ScenarioInfo> GetAsync(Guid id)
        {
            return await _sql.GetAsync<ScenarioInfo>(id);
        }

        public async Task<ScenarioInfo> GetAsync(string scenarioName)
        {
            return await _sql.GetByColumnAsync<ScenarioInfo>("ScenarioName", scenarioName);
        }

        public async Task<IReadOnlyList<ScenarioInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ScenarioInfo>(columns);
        }

        public async Task<bool> UpdateAsync(ScenarioInfo objScenarioInfo, params string[] columns)
        {
            return await _sql.UpdateAsync<ScenarioInfo>(objScenarioInfo, columns);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _sql.DeleteAsync<ScenarioInfo>(id);
        }
    }
}
