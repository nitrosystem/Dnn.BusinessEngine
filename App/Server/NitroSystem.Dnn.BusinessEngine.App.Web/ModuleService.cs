using System;
using System.Data;
using System.Web.UI;
using System.Globalization;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public class ModuleService
    {
        public static (string Preloader, string Template) RenderModule(
            Page page,
            ICacheService cacheService,
            IExecuteSqlCommand sqlCommand,
            IUnitOfWork unitOfWork,
            string baseUrl,
            bool isDashboard,
            int? siteModuleId,
            ref Guid? moduleId,
            out string scenarioName,
            string parentFolder = "")
        {
            var preloader = string.Empty;
            var template = string.Empty;

            var module = GetModuleLiteData(cacheService, sqlCommand, unitOfWork, siteModuleId, ref moduleId);
            if (module != null)
            {
                var connectionId = Guid.NewGuid();
                var rtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "b--rtl" : "";

                var scenarioFolder = StringHelper.ToKebabCase(module.ScenarioName);
                var moduleKebabName = StringHelper.ToKebabCase(module.ModuleName);
                var modulePath = $"{baseUrl}/business-engine/{scenarioFolder}/{parentFolder}{moduleKebabName}";

                var templates = GetTemplates(cacheService, module.Id.Value, modulePath, moduleKebabName);
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

        private static ModuleLiteData GetModuleLiteData(
            ICacheService cacheService,
            IExecuteSqlCommand sqlCommand,
            IUnitOfWork unitOfWork,
            int? siteModuleId,
            ref Guid? moduleId)
        {
            var cacheKey = "BE_Modules_" + (siteModuleId != null
                ? siteModuleId.Value.ToString()
                : moduleId.Value.ToString());

            var module = cacheService.Get<ModuleLiteData>(cacheKey);
            if (module == null)
            {
                var commandText = "dbo.BusinessEngine_App_GetModuleLite";
                var param = new
                {
                    SiteModuleId = siteModuleId,
                    ModuleId = moduleId
                };

                using (var reader = sqlCommand.ExecuteSqlReader(unitOfWork, CommandType.StoredProcedure, commandText, param))
                {
                    if (reader.Read())
                    {
                        module = new ModuleLiteData()
                        {
                            Id = reader["Id"] as Guid?,
                            ScenarioName = reader["ScenarioName"] as string,
                            ModuleName = reader["ModuleName"] as string,
                            ModuleVersion = reader["ModuleVersion"] as int?
                        };

                        cacheService.Set<ModuleLiteData>(cacheKey, module);
                    }
                }
            }

            moduleId = module != null
                ? module.Id
                : null;

            return module;
        }

        private static (string Preloader, string Template) GetTemplates(
            ICacheService cacheService,
            Guid moduleId,
            string modulePath,
            string moduleKebabName)
        {
            var cacheKey = "BE_Modules_Template" + moduleId;
            var data = cacheService.Get<(string Preloader, string Template)>(cacheKey);
            var preloader = data.Preloader;
            var template = data.Template;

            if (string.IsNullOrEmpty(template))
            {
                string modulePreloaderUrl = $"{modulePath}/{moduleKebabName}.preloader.html";
                string moduleTemplateUrl = $"{modulePath}/{moduleKebabName}.html";

                preloader = FileUtil.GetFileContent(Constants.MapPath(modulePreloaderUrl));
                template = FileUtil.GetFileContent(Constants.MapPath(moduleTemplateUrl));

                data = (preloader, template);
                cacheService.Set<(string Preloader, string Template)>(cacheKey, data);
            }

            return data;
        }
    }
}