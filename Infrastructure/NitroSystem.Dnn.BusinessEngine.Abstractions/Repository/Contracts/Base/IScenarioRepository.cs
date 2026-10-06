using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Base;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Base
{
  public  interface IScenarioRepository
    {
        Task<Guid> AddAsync(ScenarioInfo objScenarioInfo);

        Task<string> GetScenarioNameAsync(Guid id);
        Task<ScenarioInfo> GetAsync(Guid id);
        Task<ScenarioInfo> GetAsync(string scenarioName);
        Task<IReadOnlyList<ScenarioInfo>> GetsAsync(params string[] columns);

        Task<bool> UpdateAsync(ScenarioInfo objScenarioInfo, params string[] columns);

        Task<bool> DeleteAsync(Guid id);
    }
}
