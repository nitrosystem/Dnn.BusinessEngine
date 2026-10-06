using System;
using System.Linq;
using System.Web.Helpers;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Framework;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Dashboard : PortalModuleBase, IActionable
    {
        private readonly ICacheService _cacheService;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly IUserDataStore _userDataStore;
        private readonly ActionRunner _actionRunner;
        private readonly string _serviceRoot;
        private string _scenarioName;
        private Guid? _id;

        public Dashboard()
        {
            _serviceRoot = ServicesFramework.GetServiceFrameworkRoot();
            _cacheService = DependencyProvider.GetService<ICacheService>();
            _dashboardService = DependencyProvider.GetService<IDashboardService>();
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
                string moduleParam = _id.HasValue ? "id=" + _id.ToString() : "d=" + this.ModuleId.ToString();
                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}{1}&m=create-dashboard&{2}&ru={3}", _scenarioName, serviceRootInUrl, moduleParam, this.TabId));
            }
        }
        public bool IsAdmin
        {
            get
            {
                return this.UserInfo.IsSuperUser || this.UserInfo.IsInRole("Administrators");
            }
        }
        public int Version
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

            var currentPage = Request.QueryString["page"];
            var dashboardModule = _moduleService.GetModuleLiteData(ModuleId);
            if (dashboardModule != null)
            {
                var dashboardRoles = _dashboardService.GetDashboardAuthorization(dashboardModule.Id);
                if (!UserInfo.IsSuperUser && dashboardRoles.Any() && !UserInfo.Roles.Any(r => dashboardRoles.Contains(r)))
                    Response.Redirect(DotNetNuke.Common.Globals.AccessDeniedURL());

                _id = dashboardModule.Id;
                _scenarioName = dashboardModule.ScenarioName;

                var connectionId = Guid.NewGuid().ToString();
                var rtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                    ? "b--rtl"
                    : "";

                var basePath = PortalSettings.HomeSystemDirectoryMapPath;
                var userId = UserInfo.UserID;
                var isSuperUser = UserInfo.IsSuperUser;
                var roles = UserInfo.Roles;
                var rawUrl = Request.RawUrl;

                var templates = ModuleService.RenderModule(Page, dashboardModule, _cacheService, PortalSettings.HomeSystemDirectory, true);

                Guid? pageModuleId = null;
                var pageModuleTemplate = string.Empty;
                if (!string.IsNullOrEmpty(currentPage))
                {
                    var pageModule = _dashboardService.GetDashboardPageModule(dashboardModule.Id, currentPage);
                    if (pageModule != null)
                    {
                        var module = new ModuleLiteDto() { Id = pageModule.ModuleId, ModuleName = pageModule.ModuleName, ScenarioName = dashboardModule.ScenarioName };
                        var parentFolder = StringHelper.ToKebabCase(dashboardModule.ModuleName) + "/";
                        var childTemplates = ModuleService.RenderModule(Page, module, _cacheService, PortalSettings.HomeSystemDirectory, false, parentFolder);

                        pageModuleTemplate = childTemplates.Preloader + childTemplates.Template;
                        pageModuleTemplate = pageModuleTemplate.Replace("[CONNECTION_ID]", connectionId);
                        pageModuleTemplate = pageModuleTemplate.Replace("[RTL_CLASS]", rtlCssClass);
                        pageModuleTemplate = pageModuleTemplate.Replace("[PAGE_ICON]", pageModule.PageIcon);
                        pageModuleTemplate = pageModuleTemplate.Replace("[PAGE_TITLE]", pageModule.PageTitle);
                        pageModuleTemplate = pageModuleTemplate.Replace("[PAGE_DESCRIPTION]", pageModule.PageDescription);

                        if (pageModule.IsSSR)
                        {
                            pageModuleTemplate = ModuleService.BuildSSRTemplate(_dashboardService, _actionService, _userDataStore, _actionRunner,
                                                                    pageModuleTemplate, connectionId, basePath, rawUrl,
                                                                    userId, isSuperUser, roles, pageModule.ModuleId);
                        }

                        pageModuleId = module.Id;
                    }
                }

                var template = templates.Preloader + templates.Template;
                template = template.Replace("[CONNECTION_ID]", connectionId);
                template = template.Replace("[RTL_CLASS]", rtlCssClass);
                template = template.Replace("[USER_DISPAYNAME]", UserInfo.DisplayName);
                template = template.Replace("[USER_IMAGE]", $"/dnnimagehandler.ashx?mode=profilepic&userid={UserInfo.UserID}");
                template = template.Replace("[PAGE_MODULE]", pageModuleTemplate);

                if (dashboardModule.IsSSR)
                {
                    template = ModuleService.BuildSSRTemplate(_dashboardService, _actionService, _userDataStore, _actionRunner,
                                template, connectionId, basePath, rawUrl, userId, isSuperUser, roles, dashboardModule.Id, true, currentPage);
                }

                pnlTemplate.InnerHtml = template;

                CtlPageResource.DnnTabId = TabId;
                CtlPageResource.ModuleIds = new HashSet<Guid>(new Guid[1] { _id.Value });
                if (pageModuleId.HasValue) CtlPageResource.ModuleIds.Add(pageModuleId.Value);
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
                actions.Add(GetNextActionID(), "Create Dashboard", "Create.Dashboard", "", "~/DesktopModules/BusinessEngine/assets/images/dashboard-16.png", StudioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion
    }
}