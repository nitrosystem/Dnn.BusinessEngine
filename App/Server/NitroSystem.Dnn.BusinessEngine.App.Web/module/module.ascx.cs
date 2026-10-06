using System;
using System.Web.Helpers;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Framework;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
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
        private readonly string _serviceRoot;
        private string _scenarioName;
        private Guid? _moduleId;

        public Module()
        {
            _serviceRoot = ServicesFramework.GetServiceFrameworkRoot();
            _cacheService = DependencyProvider.GetRequiredService<ICacheService>();
            _moduleService = DependencyProvider.GetRequiredService<IModuleService>();
            _actionService = DependencyProvider.GetRequiredService<IActionService>();
            _userDataStore = DependencyProvider.GetRequiredService<IUserDataStore>();
            _actionRunner = DependencyProvider.GetRequiredService<ActionRunner>();
        }

        #region Properties

        public string ServiceRoot
        {
            get
            {
                return _serviceRoot;
            }
        }

        public string StudioUrl
        {
            get
            {
                string serviceRootInUrl = _serviceRoot == "/"
                ? string.Empty
                : "&sr=" + _serviceRoot;
                string moduleParam = _moduleId.HasValue ? "id=" + _moduleId.ToString() : "d=" + ModuleId.ToString();
                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}{1}&m=create-module&{2}&ru={3}", _scenarioName, serviceRootInUrl, moduleParam, TabId));
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
                var isSuperUser = UserInfo.IsSuperUser;
                var roles = UserInfo.Roles;
                var rawUrl = Request.RawUrl;

                var templates = ModuleService.RenderModule(Page, module, _cacheService, basePath);
                var template = templates.Preloader + templates.Template;
                template = template.Replace("[CONNECTION_ID]", connectionId);
                template = template.Replace("[RTL_CLASS]", rtlCssClass);

                if (module.IsSSR)
                {
                    template = ModuleService.BuildSSRTemplate(null, _actionService, _userDataStore, _actionRunner, template,
                                                connectionId, basePath, rawUrl, userId, isSuperUser, roles, module.Id);
                }

                pnlTemplate.InnerHtml = template;

                CtlPageResource.DnnTabId = TabId;
                CtlPageResource.ModuleIds = new HashSet<Guid>(new Guid[1] { _moduleId.Value });
                CtlPageResource.RegisterPageResources(_moduleService);
            }
        }

        #endregion

        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Module Builder", "Module.Builder", "", "~/DesktopModules/BusinessEngine/assets/images/module-16.png", StudioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion
    }
}