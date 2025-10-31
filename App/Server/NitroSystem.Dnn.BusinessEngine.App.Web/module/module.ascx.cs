using System;
using System.Web.UI;
using System.Web.Helpers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Caching;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Module : PortalModuleBase, IActionable
    {
        private readonly ICacheService _cacheService;
        private readonly string _siteRoot;
        private string _scenarioName;
        private Guid? _id;

        public Module()
        {
            _cacheService = new CacheService();

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

        #endregion

        #region Event Handlers

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));

            var templates = ModuleService.RenderModule(this.Page, _cacheService, PortalSettings.HomeSystemDirectory, false, ModuleId, ref _id, out _scenarioName);
            pnlTemplate.InnerHtml = templates.Template;

            if (_id.HasValue)
            {
                CtlPageResource.DnnTabId = TabId;
                CtlPageResource.ModuleIds = new HashSet<Guid>(new Guid[1] { _id.Value });
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
                actions.Add(GetNextActionID(), "Module Builder", "Module.Builder", "", "~/DesktopModules/BusinessEngine/assets/icons/module-builder-16.png", StudioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion
    }
}