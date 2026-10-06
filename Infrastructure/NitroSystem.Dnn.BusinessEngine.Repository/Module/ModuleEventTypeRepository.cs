using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ORM.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Repository.Module
{
    public class ModuleEventTypeRepository : IModuleEventTypeRepository
    {
        private readonly ISql _sql;

        public ModuleEventTypeRepository(ISql sql)
        {
            _sql = sql;
        }

        public async Task<IEnumerable<ModuleEventTypeInfo>> GetsAsync(params string[] columns)
        {
            return await _sql.GetAllAsync<ModuleEventTypeInfo>(columns);
        }
    }
}
