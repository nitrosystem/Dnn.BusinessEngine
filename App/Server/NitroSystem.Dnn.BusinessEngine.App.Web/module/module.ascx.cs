using System;
using System.Web.Helpers;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Framework;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Module : PortalModuleBase, IActionable
    {
        private readonly ICacheService _cacheService;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly IUserDataStore _userDataStore;
        private readonly ActionRunner _actionRunner;
        private readonly string _siteRoot;
        private string _scenarioName;
        private Guid? _moduleId;

        public Module()
        {
            var root = ServicesFramework.GetServiceFrameworkRoot();

            _cacheService = DependencyProvider.GetRequiredService<ICacheService>();
            _moduleService = DependencyProvider.GetRequiredService<IModuleService>();
            _actionService = DependencyProvider.GetRequiredService<IActionService>();
            _userDataStore = DependencyProvider.GetRequiredService<IUserDataStore>();
            _actionRunner = DependencyProvider.GetRequiredService<ActionRunner>();

            _siteRoot = root == "/"
                ? string.Empty
                : "&sr=" + root;
        }

        #region Properties

        public string StudioUrl
        {
            get
            {
                string moduleParam = _moduleId.HasValue ? "id=" + _moduleId.ToString() : "d=" + ModuleId.ToString();
                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}{1}&m=create-module&{2}&ru={3}", _scenarioName, _siteRoot, moduleParam, TabId));
            }
        }

        public bool IsAdmin
        {
            get
            {
                return UserInfo.IsSuperUser || UserInfo.IsInRole("Administrators");
            }
        }

        public int HostVersion
        {
            get
            {
                return Host.CrmVersion;
            }
        }

        #endregion

        #region Event Handlers

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new System.Web.UI.LiteralControl(code));

            var module = _moduleService.GetModuleLiteData(ModuleId, _moduleId);
            if (module != null)
            {
                _moduleId = module.Id;
                _scenarioName = module.ScenarioName;

                var connectionId = Guid.NewGuid().ToString();
                var rtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                    ? "b--rtl"
                    : "";

                var basePath = PortalSettings.HomeSystemDirectoryMapPath;
                var userId = UserInfo.UserID;

                var templates = ModuleService.RenderModule(Page, module, _cacheService, basePath);
                var template = templates.Preloader + templates.Template;
                template = template.Replace("[CONNECTION_ID]", connectionId);
                template = template.Replace("[RTL_CLASS]", rtlCssClass);

                if (module.IsSSR)
                    template = buildSSRTemplate(template, connectionId, basePath, userId);

                pnlTemplate.InnerHtml = template;

                CtlPageResource.DnnTabId = TabId;
                CtlPageResource.ModuleIds = new HashSet<Guid>(new Guid[1] { _moduleId.Value });
                CtlPageResource.RegisterPageResources();
            }
        }

        #endregion

        private async Task<ConcurrentDictionary<string, object>> GetDataForSSR(string connectionId, string basePath, int userId)
        {
            var moduleData = await _userDataStore.GetOrCreateModuleDataAsync(connectionId, _moduleId.Value, basePath);
            var actions = await _actionService.GetActionsAsync(_moduleId.Value);
            if (actions.Count > 0)
                await _actionRunner.ExecuteAsync(
                   actions,
                   connectionId,
                   _moduleId.Value,
                   userId,
                   this.Request.RawUrl,
                   basePath,
                   moduleData);

            var data = _userDataStore.GetDataForClients(connectionId, _moduleId.Value) ?? new ConcurrentDictionary<string, object>();
            return data;
        }

        private string buildSSRTemplate(string template, string connectionId, string basePath, int userId)
        {
            var data = AsyncHelper.RunSync(() => GetDataForSSR(connectionId, basePath, userId));

            var parser = new TemplateParser();
            var ast = parser.Parse(template);
            var context = new RenderContext(data);
            var builder = new BuildTemplate();

            template = builder.Render(ast, context);
            return template;
        }

        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Module Builder", "Module.Builder", "", "~/DesktopModules/BusinessEngine/assets/icons/module-builder-16.png", StudioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion
    }
}