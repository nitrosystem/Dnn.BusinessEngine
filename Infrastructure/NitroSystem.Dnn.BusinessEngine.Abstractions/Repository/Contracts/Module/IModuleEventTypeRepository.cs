using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleEventTypeRepository
    {
        Task<IEnumerable<ModuleEventTypeInfo>> GetsAsync(params string[] columns);
    }
}
