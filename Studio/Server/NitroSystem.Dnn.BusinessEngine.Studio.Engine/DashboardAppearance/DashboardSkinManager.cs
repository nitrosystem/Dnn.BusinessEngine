using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.DashboardAppearance
{
    public static class DashboardSkinManager
    {
        public static Models.DashboardSkin GetDashboardSkin(
            DashboardType dashboardType, string skinName, string skinPath, HttpContext context)
        {
            var cacheService = new CacheServiceBase();
            var cacheKey = "BE_Dashboard_Skins_" + skinName;
            var result = cacheService.Get<Models.DashboardSkin>(cacheKey);
            if (result == null)
            {
                string modulePath = "~/DesktopModules/BusinessEngine";
                string path = skinPath.Replace("[ModulePath]", modulePath);
                string skinJson = FileUtil.GetFileContent(context.Request.MapPath(path + "/skin.json"));

                result = JsonConvert.DeserializeObject<DashboardSkin>(skinJson);

                string cleanedModulePath = modulePath.TrimStart('~');
                result.SkinPath = path;

                var typeMapping = new Dictionary<DashboardType, string>
            {
                { DashboardType.StandalonePanel, "Standalone" },
                { DashboardType.Dnn, "Dnn" }
            };

                if (result.DashboardTemplates != null && typeMapping.TryGetValue(dashboardType, out var type))
                {
                    result.DashboardTemplates = result.DashboardTemplates
                        .Where(t => t.DashboardType == type)
                        .Select(t =>
                        {
                            if (!string.IsNullOrEmpty(t.TemplateImage))
                                t.TemplateImage = t.TemplateImage.Replace("[ModulePath]", cleanedModulePath);
                            return t;
                        })
                        .ToList();
                }

                cacheService.Set<Models.DashboardSkin>(cacheKey, result);
            }

            return result;
        }

        public static IEnumerable<DashboardTemplate> GetDashboardTemplates(
          DashboardType dashboardType, string skinName, string skinPath, HttpContext context)
        {
            var dashboardSkin = GetDashboardSkin(dashboardType, skinName, skinPath, context);
            return dashboardSkin?.DashboardTemplates ?? Enumerable.Empty<DashboardTemplate>();
        }

        public static IEnumerable<ModuleTemplate> GetModuleTemplates(
          DashboardType dashboardType, ModuleType moduleType, string skinName, string skinPath, HttpContext context)
        {
            var dashboardSkin = GetDashboardSkin(dashboardType, skinName, skinPath, context);
            switch (moduleType)
            {
                case ModuleType.Form:
                    return dashboardSkin.FormTemplates;
                case ModuleType.List:
                    return dashboardSkin.ListTemplates;
                case ModuleType.Details:
                    return dashboardSkin.DetailsTemplates;
                default:
                    return Enumerable.Empty<ModuleTemplate>();
            }
        }

        public static string GetDashboardTemplateHtml(
            DashboardType dashboardType, string skinName, string skinPath, string template, HttpContext context)
        {
            var skin = GetDashboardSkin(dashboardType, skinName, skinPath, context);

            string templatePath = skin.DashboardTemplates?.FirstOrDefault(t => t.TemplateName == template).TemplatePath;
            string modulePath = "~/DesktopModules/BusinessEngine";
            templatePath = templatePath.Replace("[ModulePath]", modulePath);

            string layoutTemplateFileContent = FileUtil.GetFileContent(context.Server.MapPath(templatePath));

            return layoutTemplateFileContent;
        }
    }
}
