using System;
using System.Globalization;
using System.Web.UI;
using System.Web.Helpers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using System.Linq.Expressions;
using System.Reflection;
using DotNetNuke.Data;
using System.Data.SqlClient;
using System.Data;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using NitroSystem.Dnn.BusinessEngine.Common.IO;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Module : PortalModuleBase, IActionable
    {
        #region Properties

        public string ScenarioName { get; set; }

        public int ModulePortalId { get; set; }

        public Guid? ModuleGuid { get; set; }

        public string ModuleName { get; set; }

        public int ModuleVersion { get; set; }

        public string SiteRoot
        {
            get
            {
                string domainName = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Context.Request)) + "/";
                return domainName;
            }
        }

        public bool IsModuleAllTabs
        {
            get
            {
                return this.ModuleConfiguration.AllTabs;
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

        public string ConnectionId
        {
            get
            {
                return Request.AnonymousID;
            }
        }

        public string StudioUrl
        {
            get
            {
                string moduleParamValue = this.ModuleGuid == null ? this.ModuleId.ToString() : this.ModuleGuid.ToString();

                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio/studio.aspx?s={0}&p={1}&a={2}&m=create-module&id={3}&ru={4}", this.ScenarioName, this.PortalId, this.PortalAlias.PortalAliasID, moduleParamValue, this.TabId));
            }
        }

        #endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            lnkModuleBuilder.Visible = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole("Administrators");
            lnkModuleBuilder.PostBackUrl = this.StudioUrl;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));

            FillData();

            if (this.ModuleGuid != null)
            {
                CtlPageResource.PortalAlias = this.PortalAlias.HTTPAlias;
                CtlPageResource.DnnTabId = this.TabId;
                CtlPageResource.DnnUserId = this.UserId;
                CtlPageResource.DnnUserDisplayName = this.UserInfo.DisplayName;
                CtlPageResource.ModuleGuid = this.ModuleGuid;
                CtlPageResource.ModuleName = this.ModuleName;
                CtlPageResource.IsModuleInAllTabs = this.IsModuleAllTabs;
                CtlPageResource.RegisterPageResources();

                var templates = GetTemplates();
                pnlTemplate.InnerHtml = templates.Template;
            }
        }

        #endregion

        private void FillData()
        {
            using (var connection = new SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("dbo.BusinessEngine_GetModuleLite", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DnnModuleId", this.ModuleId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            this.ScenarioName = reader["ScenarioName"] as string;
                            this.ModuleGuid = reader["Id"] as Guid?;
                            this.ModuleName = reader["ModuleName"] as string;
                            this.ModuleVersion = (int)reader["ModuleVersion"];
                        }
                    }
                }

                connection.Close();
            }
        }

        private (string Preloader, string Template) GetTemplates()
        {
            string Preloader = string.Empty;
            string template = string.Empty;

            string modulePath = (this.ModulePortalId == this.PortalId ? this.PortalSettings.HomeSystemDirectory : new PortalSettings(this.ModulePortalId).HomeSystemDirectory) + @"business-engine/";
            string moduleTemplateUrl = string.Format("{0}/{1}/{2}/module--{2}.html", modulePath, this.ScenarioName, this.ModuleName);
            template = FileUtil.GetFileContent(MapPath(moduleTemplateUrl));

            return (Preloader, template);
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