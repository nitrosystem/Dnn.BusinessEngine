using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule
{
    public static class CleanPageResources
    {
        public static IEnumerable<PageResourceDto> Clean(
            int? pageId,
            bool isStandaloneDashboard,
            List<ModuleResourceInfo> resources,
            IEnumerable<PageResourceDto> pageResources)
        {
            if (isStandaloneDashboard)
            {
                return resources.Select(r => new PageResourceDto
                {
                    ModuleId = r.ModuleId,
                    DnnPageId = null,
                    IsCustomResource = false,
                    ResourceType = r.ResourceType,
                    ResourcePath = r.ResourcePath,
                    IsActive = true,
                    LoadOrder = r.LoadOrder
                });
            }

            var existingResourcesSet = new HashSet<string>(
                pageResources
                    .Where(r => !r.IsCustomResource) 
                    .Select(r => $"{r.ResourceType}|{r.ResourcePath}"),
                StringComparer.OrdinalIgnoreCase
            );

            var cleaned = resources
                .Select(r => new PageResourceDto
                {
                    ModuleId = r.ModuleId,
                    DnnPageId = pageId,
                    IsCustomResource = false,
                    ResourceType = r.ResourceType,
                    ResourcePath = r.ResourcePath,
                    IsActive = true,
                    LoadOrder = r.LoadOrder
                })
                .Where(r =>
                    r.IsCustomResource ||
                    !existingResourcesSet.Contains($"{r.ResourceType}|{r.ResourcePath}")
                );

            return cleaned;
        }
    }
}
