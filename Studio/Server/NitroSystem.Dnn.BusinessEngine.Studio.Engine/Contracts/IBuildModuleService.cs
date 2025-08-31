using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IBuildModuleService
    {
        Task<PrebuildResultDto> PrepareBuild(BuildModuleDto module, IRepositoryBase repository, PortalSettings portalSettings);
        Task<IEnumerable<PageResourceDto>> ExecuteBuildAsync(BuildModuleDto module, IEnumerable<BuildModuleFieldDto> fields, IEnumerable<BuildModuleResourceDto> resources, int pageId, PortalSettings portalSettings, HttpContext context);
    }
}
