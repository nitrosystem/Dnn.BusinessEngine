using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IBuildModuleService
    {
        Task<PrebuildResultDto> PrepareBuild(BuildModuleDto module, IRepositoryBase repository, PortalSettings portalSettings);
        Task<IEnumerable<PageResourceDto>> ExecuteBuildAsync(
            BuildModuleDto module,
            IEnumerable<BuildModuleFieldDto> fields,
            IEnumerable<BuildModuleResourceDto> resources,
            int pageId,
            PortalSettings portalSettings,
            HttpContext context);
    }
}
