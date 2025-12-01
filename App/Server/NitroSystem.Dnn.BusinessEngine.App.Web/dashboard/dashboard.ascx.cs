using System;
using System.Data;
using System.Web.Helpers;
using System.Web.UI;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Framework;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Dashboard : PortalModuleBase, IActionable
    {
        private readonly ICacheService _cacheService;
        private readonly IDashboardService _dashboardService;
        private readonly IModuleService _moduleService;
        private readonly string _siteRoot;
        private string _scenarioName;
        private Guid? _id;

        public Dashboard()
        {
            _cacheService = DependencyProvider.GetService<ICacheService>();
            _dashboardService = DependencyProvider.GetService<IDashboardService>();
            _moduleService = DependencyProvider.GetRequiredService<IModuleService>();

            var root = ServicesFramework.GetServiceFrameworkRoot();
            _siteRoot = root == "/"
                ? string.Empty
                : "&sr=" + root;
        }

        #region Properties

        public string StudioUrl
        {
            get
            {
                string moduleParam = _id.HasValue ? "id=" + _id.ToString() : "d=" + this.ModuleId.ToString();

                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}{1}&m=create-dashboard&{2}&ru={3}", _scenarioName, _siteRoot, moduleParam, this.TabId));
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
            pnlAntiForgery.Controls.Add(new LiteralControl(code));

            var dashboardModule = _moduleService.GetModuleLiteData(ModuleId);
            if (dashboardModule != null)
            {
                _id = dashboardModule.Id;
                _scenarioName = dashboardModule.ScenarioName;

                var templates = ModuleService.RenderModule(Page, dashboardModule, _cacheService, PortalSettings.HomeSystemDirectory, true);

                Guid? pageModuleId = null;
                var pageModuleTemplate = string.Empty;
                var pageName = Request.QueryString["page"];
                if (!string.IsNullOrEmpty(pageName))
                {
                    var pageModule = _dashboardService.GetDashboardPageModule(dashboardModule.Id, pageName);
                    if (pageModule != null)
                    {
                        var module = new ModuleLiteDto() { Id = pageModule.ModuleId, ModuleName = pageModule.ModuleName, ScenarioName = dashboardModule.ScenarioName };
                        var parentFolder = StringHelper.ToKebabCase(dashboardModule.ModuleName) + "/";
                        var childTemplates = ModuleService.RenderModule(Page, module, _cacheService, PortalSettings.HomeSystemDirectory, false, parentFolder);

                        pageModuleTemplate = childTemplates.Template;
                        pageModuleTemplate = pageModuleTemplate.Replace("[PAGE_ICON]", pageModule.PageIcon);
                        pageModuleTemplate = pageModuleTemplate.Replace("[PAGE_TITLE]", pageModule.PageTitle);
                        pageModuleTemplate = pageModuleTemplate.Replace("[PAGE_DESCRIPTION]", pageModule.PageDescription);

                        pageModuleId = module.Id;
                    }
                }

                var template = templates.Template;
                template = template.Replace("[USER_DISPAYNAME]", UserInfo.DisplayName);
                template = template.Replace("[USER_IMAGE]", $"/dnnimagehandler.ashx?mode=profilepic&userid={UserInfo.UserID}");
                template = template.Replace("[PAGE_MODULE]", pageModuleTemplate);
                pnlTemplate.InnerHtml = template;

                CtlPageResource.DnnTabId = TabId;
                CtlPageResource.ModuleIds = new HashSet<Guid>(new Guid[1] { _id.Value });

                if (pageModuleId.HasValue)
                    CtlPageResource.ModuleIds.Add(pageModuleId.Value);

                CtlPageResource.RegisterPageResources();
            }
        }

        #endregion

        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Create Dashboard", "Create.Dashboard", "", "~/DesktopModules/BusinessEngine/assets/icons/module-builder-16.png", StudioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion
    }
}