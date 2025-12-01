using System;
using System.Web.UI;
using System.Globalization;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public class ModuleService
    {
        public static (string Preloader, string Template) RenderModule(
            Page page,
            ModuleLiteDto module,
            ICacheService cacheService,
            string baseUrl,
            bool isDashboard = false,
            string parentFolder = "")
        {
            var preloader = string.Empty;
            var template = string.Empty;
            var connectionId = Guid.NewGuid();
            var rtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "b--rtl" : "";
            var scenarioFolder = StringHelper.ToKebabCase(module.ScenarioName);
            var moduleKebabName = StringHelper.ToKebabCase(module.ModuleName);
            var modulePath = $"{baseUrl}/business-engine/{scenarioFolder}/{parentFolder}{moduleKebabName}";

            var cacheKey = "BE_Modules_Template" + module.Id;
            var data = cacheService.Get<(string Preloader, string Template)>(cacheKey);
            preloader = data.Preloader;
            template = data.Template;
            if (string.IsNullOrEmpty(template))
            {
                string modulePreloaderUrl = $"{modulePath}/{moduleKebabName}.preloader.html";
                string moduleTemplateUrl = $"{modulePath}/{moduleKebabName}.html";

                preloader = FileUtil.GetFileContent(Constants.MapPath(modulePreloaderUrl));
                template = FileUtil.GetFileContent(Constants.MapPath(moduleTemplateUrl));
                template = $@"
                <div b-controller=""moduleController"" data-module=""{module.Id}"" data-dashboard=""{isDashboard}"" data-connection=""{connectionId}"" class=""b--module {rtlCssClass}"">
                    {template}
                </div>";

                data = (preloader, template);
                cacheService.Set<(string Preloader, string Template)>(cacheKey, data);
            }

            return (preloader, template);
        }
    }
}