using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Globals;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine;
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

			if (!module.IsSSR)
			{
				var data = cacheService.Get<(string Preloader, string Template)>(cacheKey);
				preloader = data.Preloader;
				template = data.Template;
			}

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

				if (!module.IsSSR)
				{
					cacheService.Set<(string Preloader, string Template)>(cacheKey, (preloader, template));
				}
			}

			return (preloader, template);
		}

		public static string BuildSSRTemplate(
			IDashboardService dashboardService,
			IActionService actionService,
			IUserDataStore userDataStore,
			ActionRunner actionRunner,
			string template,
			string connectionId,
			string basePath,
			string rawUrl,
			int userId,
			bool isSuperUser,
			string[] roles,
			Guid moduleId,
			bool isDashboard = false,
			string currentPage = "")
		{
			var data = AsyncHelper.RunSync(() => GetDataForSSR(dashboardService, actionService, userDataStore, actionRunner,
													connectionId, basePath, rawUrl, userId, isSuperUser, roles, moduleId, isDashboard, currentPage));
			var context = new TemplateContext(data);
			var builder = new BuildTemplate();
			var parser = new TemplateParser();
			var parsedTemplate = parser.Parse(template);
			template = builder.Render(parsedTemplate, context);
			return template;
		}

		private static async Task<ConcurrentDictionary<string, object>> GetDataForSSR(
			IDashboardService dashboardService,
			IActionService actionService,
			IUserDataStore userDataStore,
			ActionRunner actionRunner,
			string connectionId,
			string basePath,
			string rawUrl,
			int userId,
			bool isSuperUser,
			string[] roles,
			Guid moduleId,
			bool isDashboard,
			string currentPage)
		{
			var moduleData = await userDataStore.GetOrCreateModuleDataAsync(connectionId, moduleId, basePath);
			var actions = await actionService.GetActionsAsync(moduleId);
			if (actions.Count > 0)
				await actionRunner.ExecuteAsync(
				actions,
				connectionId,
				moduleId,
				rawUrl,
				basePath,
				userId,
				isSuperUser,
				roles,
				moduleData);

			var data = userDataStore.GetDataForClients(connectionId, moduleId) ?? new ConcurrentDictionary<string, object>();

			if (isDashboard)
			{
				var pages = await dashboardService.GetDashboardPagesDtoAsync(moduleId, isSuperUser, roles, currentPage);
				data.TryAdd("_DashboardPages", pages);
			}

			return data;
		}
	}
}