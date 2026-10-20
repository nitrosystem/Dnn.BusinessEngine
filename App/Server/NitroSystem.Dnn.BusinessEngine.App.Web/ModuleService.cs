using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
	public static class ModuleService
	{
		public static (string Preloader, string Template) RenderModule(
			System.Web.UI.Page page,
			ModuleLiteDto module,
			ICacheService cacheService,
			string baseUrl,
			bool isDashboard = false,
			string parentFolder = "")
		{
			var preloader = string.Empty;
			var template = string.Empty;
			var scenarioFolder = StringHelper.ToKebabCase(module.ScenarioName);
			var moduleKebabName = StringHelper.ToKebabCase(module.ModuleName);
			var modulePath = $"{baseUrl}/business-engine/{scenarioFolder}/{parentFolder}{moduleKebabName}";

			var cacheKey = "BE_Modules_App__Template" + module.Id;

			if (string.IsNullOrEmpty(template))
			{
				string modulePreloaderUrl = $"{modulePath}/{moduleKebabName}.preloader.html";
				string moduleTemplateUrl = $"{modulePath}/{moduleKebabName}.html";

				preloader = FileUtil.GetFileContent(GlobalHelper.MapPath(modulePreloaderUrl));
				template = FileUtil.GetFileContent(GlobalHelper.MapPath(moduleTemplateUrl));
				template = $@"
                <div b-controller=""moduleController"" data-module=""{module.Id}"" data-dashboard=""{isDashboard}"" data-connection=""[CONNECTION_ID]"" class=""b--module [RTL_CLASS]"">
                    {template}
                </div>";
			}

			return (preloader, template);
		}
	}
}