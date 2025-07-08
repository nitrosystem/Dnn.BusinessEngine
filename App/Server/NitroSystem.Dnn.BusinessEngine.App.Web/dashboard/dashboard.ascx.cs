using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Infrastructure.SSR;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.ModuleBuilder;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using NitroSystem.Dnn.BusinessEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Dashboard : PortalModuleBase, IActionable
    {
        #region Properties

        public Guid? ModuleGuid
        {
            get
            {
                return ModuleRepository.Instance.GetModuleGuidByDnnModuleID(this.ModuleId) ?? null;
            }
        }

        public int DashboardType
        {
            get
            {
                return this.ModuleGuid == null ? 0 : DashboardRepository.Instance.GetDashboardType(this.ModuleGuid.Value);
            }
        }

        public string ModuleName
        {
            get
            {
                return this.ModuleGuid != null ? ModuleRepository.Instance.GetModuleName(this.ModuleGuid.Value) : "";
            }
        }

        public string ScenarioName
        {
            get
            {
                return this.ModuleGuid != null ? ModuleRepository.Instance.GetModuleScenarioName(this.ModuleGuid.Value) : "";
            }
        }

        public string SiteRoot
        {
            get
            {
                string domainName = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Context.Request)) + "/";
                return domainName;
            }
        }

        public bool IsRtl
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
            }
        }

        public string bRtlCssClass
        {
            get
            {
                return IsRtl ? "b-rtl" : "";
            }

        }

        public string ConnectionID
        {
            get
            {
                return Request.AnonymousID;
            }
        }

        public string ControlPanelUrl
        {
            get
            {
                string moduleParam = this.ModuleGuid == null ? "d" : "id";
                string moduleParamValue = this.ModuleGuid == null ? this.ModuleId.ToString() : this.ModuleGuid.ToString();

                return ResolveUrl($"~/DesktopModules/BusinessEngine/Studio.aspx?s={this.ScenarioName}&p={this.PortalId}&a={this.PortalAlias.PortalAliasID}&{moduleParam}={moduleParamValue}&m=create-dashboard&ru={this.TabId}");
            }
        }

        #endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            lnkDashboardBuilder.Visible = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole("Administrators");
            lnkDashboardBuilder.PostBackUrl = ControlPanelUrl;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));

            if (this.ModuleGuid != null)
            {
                var dashboard = DashboardRepository.Instance.GetDashboardViewByModuleID(this.ModuleGuid.Value);
                if (dashboard.DashboardType == 2)
                {
                    CtlPageResource.PortalAlias = this.PortalAlias.HTTPAlias;
                    CtlPageResource.DnnTabID = this.TabId;
                    CtlPageResource.DnnUserID = this.UserId;
                    CtlPageResource.DnnUserDisplayName = this.UserInfo.DisplayName;
                    CtlPageResource.ModuleGuid = this.ModuleGuid;
                    CtlPageResource.ModuleName = this.ModuleName;

                    CtlPageResource.RegisterPageResources();
                }
            }
        }

        #endregion

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Dashboard Builder", "Dashboard.Builder", "", "~/DesktopModules/BusinessEngine/assets/icons/module-builder-16.png", ControlPanelUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }
    }
}