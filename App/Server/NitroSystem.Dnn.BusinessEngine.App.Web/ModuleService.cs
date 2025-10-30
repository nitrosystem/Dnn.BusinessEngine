using System;
using System.Data;
using System.Web.UI;
using System.Globalization;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public class ModuleService
    {
        public static (string Preloader, string Template) RenderModule(Page page, string baseUrl, bool isDashboard, int? siteModuleId, ref Guid? moduleId, out string scenarioName, string parentFolder = "")
        {
            var preloader = string.Empty;
            var template = string.Empty;

            var module = GetModuleLiteData(siteModuleId, ref moduleId);
            if (module != null)
            {
                var connectionId = Guid.NewGuid();
                var rtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "b--rtl" : "";

                var scenarioFolder = StringHelper.ToKebabCase(module.ScenarioName);
                var moduleKebabName = StringHelper.ToKebabCase(module.ModuleName);
                var modulePath = $"{baseUrl}/business-engine/{scenarioFolder}/{parentFolder}{moduleKebabName}";

                var templates = GetTemplates(module.Id.Value, modulePath, moduleKebabName);
                template = $@"
                <div b-controller=""moduleController"" data-module=""{module.Id}"" data-dashboard=""{isDashboard}"" data-connection=""{connectionId}"" class=""b--module {rtlCssClass}"">
                    {templates.Template}
                </div>";
            }

            moduleId = module != null
                ? module.Id
                : null;

            scenarioName = module != null
                ? module.ScenarioName
                : string.Empty;

            return (preloader, template);
        }

        private static ModuleLiteData GetModuleLiteData(int? siteModuleId, ref Guid? moduleId)
        {
            var cacheKey = "BE_Modules_" + (siteModuleId != null
                ? siteModuleId.Value.ToString()
                : moduleId.Value.ToString());

            var module = DataCache.GetCache<ModuleLiteData>(cacheKey);
            if (module == null)
            {
                var reader = ExecuteSqlCommand.ExecuteSqlReader(CommandType.StoredProcedure, "dbo.BusinessEngine_App_GetModuleLite",
                    new Dictionary<string, object>()
                    {
                        {"@SiteModuleId", siteModuleId },
                        {"@ModuleId", moduleId }
                    });

                if (reader.Read())
                {
                    module = new ModuleLiteData()
                    {
                        Id = reader["Id"] as Guid?,
                        ScenarioName = reader["ScenarioName"] as string,
                        ModuleName = reader["ModuleName"] as string,
                        ModuleVersion = reader["ModuleVersion"] as int?
                    };

                    DataCache.SetCache(cacheKey, module);

                    reader.Close();
                }
            }

            moduleId = module != null
                ? module.Id
                : null;

            return module;
        }

        private static (string Preloader, string Template) GetTemplates(Guid moduleId, string modulePath, string moduleKebabName)
        {
            var cacheKey = "BE_Modules_Template" + moduleId;
            var data = DataCache.GetCache<(string Preloader, string Template)>(cacheKey);
            var preloader = data.Preloader;
            var template = data.Template;

            if (1 == 1 || string.IsNullOrEmpty(template))
            {
                string modulePreloaderUrl = $"{modulePath}/{moduleKebabName}.preloader.html";
                string moduleTemplateUrl = $"{modulePath}/{moduleKebabName}.html";

                preloader = FileUtil.GetFileContent(Constants.MapPath(modulePreloaderUrl));
                template = FileUtil.GetFileContent(Constants.MapPath(moduleTemplateUrl));

                data = (preloader, template);
                DataCache.SetCache(cacheKey, data);
            }

            return data;
        }
    }
}