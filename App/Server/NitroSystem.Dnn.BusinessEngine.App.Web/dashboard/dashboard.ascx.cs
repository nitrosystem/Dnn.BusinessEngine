using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using System;
using System.Web.Helpers;
using System.Web.UI;
using DotNetNuke.Framework;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Dashboard : PortalModuleBase, IActionable
    {
        #region Properties

        private Guid? _id { get; set; }

        private string _scenarioName { get; set; }

        private string _moduleName { get; set; }

        private string _siteRoot
        {
            get
            {
                var siteRoot = ServicesFramework.GetServiceFrameworkRoot();
                return siteRoot == "/"
                    ? string.Empty
                    : "&sr=" + siteRoot;
            }
        }

        private string _studioUrl
        {
            get
            {
                string moduleParam = _id.HasValue ? "id=" + _id.ToString() : "d=" + this.ModuleId.ToString();

                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}{1}&m=create-dashboard&{2}&ru={3}", _scenarioName, _siteRoot, moduleParam, this.TabId));
            }
        }

        #endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            lnkDashboardBuilder.Visible = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole("Administrators");
            lnkDashboardBuilder.PostBackUrl = _studioUrl;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));

            if (_id.HasValue)
            {
                //CtlPageResource.PortalAlias = this.PortalAlias.HTTPAlias;
                //CtlPageResource.DnnTabID = this.TabId;
                //CtlPageResource.DnnUserID = this.UserId;
                //CtlPageResource.DnnUserDisplayName = this.UserInfo.DisplayName;
                //CtlPageResource.ModuleGuid = this.ModuleGuid;
                //CtlPageResource.ModuleName = this.ModuleName;

                //CtlPageResource.RegisterPageResources();
            }
        }

        #endregion

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Dashboard Builder", "Dashboard.Builder", "", "~/DesktopModules/BusinessEngine/assets/icons/module-builder-16.png", _studioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }
    }
}