using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildModuleService
    {
        Task<PrebuildResultDto> PrepareBuild(BuildModuleDto module, IRepositoryBase repository, PortalSettings portalSettings);
        Task<IEnumerable<PageResourceDto>> ExecuteBuildAsync(int? pageId, (DashboardType DashboardType, string Skin, string SkinPath) dashboard, IEnumerable<BuildModuleDto> modulesToBuild, IEnumerable<BuildModuleFieldDto> fieldsToBuild, IEnumerable<BuildModuleResourceDto> resourcesToBuild, IEnumerable<PageResourceDto> oldReesources, PortalSettings portalSettings, HttpContext context);
    }
}
