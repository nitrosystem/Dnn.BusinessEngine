using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Contracts.Module
{
    public interface IModuleOutputResourceRepository
    {
        Task BulkInsertAsync(IReadOnlyList<ModuleOutputResourceInfo> resources);

        IReadOnlyList<ModuleOutputResourceInfo> Gets(int sitePageId, string moduleIds);

        Task DeleteAsync(Guid moduleId);
    }
}
