using System.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Contracts
{
    public interface IBuildModule
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
