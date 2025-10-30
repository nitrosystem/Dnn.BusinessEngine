using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using System;
using System.Web.Helpers;
using System.Web.UI;
using DotNetNuke.Framework;
using DotNetNuke.Data;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.Data.Repository;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Dashboard : PortalModuleBase, IActionable
    {
        private string _siteRoot;
        private string _scenarioName;
        private Guid? _id;

        public Dashboard()
        {
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

            Guid? pageModuleId = null;
            var pageModuleName = "";
            var pageModuleTemplate = string.Empty;
            var pageName = Request.QueryString["page"];
            if (!string.IsNullOrEmpty(pageName))
            {
                var reader = ExecuteSqlCommand.ExecuteSqlReader(CommandType.Text,
                     "SELECT ModuleId,ParentModuleName FROM dbo.BusinessEngineView_DashboardPageModules WHERE SiteModuleId = @SiteModuleId AND PageName = @PageName",
                     new Dictionary<string, object>()
                     {
                            { "@SiteModuleId", ModuleId },
                            { "@PageName", pageName }
                     });

                if (reader.Read())
                {
                    pageModuleId = reader["ModuleId"] as Guid?;
                    pageModuleName = reader["ParentModuleName"] as string;

                    var parentFolder = StringHelper.ToKebabCase(pageModuleName) + "/";
                    var childTemplates = ModuleService.RenderModule(this.Page, PortalSettings.HomeSystemDirectory, false, null, ref pageModuleId, out _scenarioName, parentFolder);
                    pageModuleTemplate = childTemplates.Template;

                    reader.Close();
                }
            }

            var templates = ModuleService.RenderModule(this.Page, PortalSettings.HomeSystemDirectory, true, ModuleId, ref _id, out _scenarioName);
            var template = templates.Template;
            template = template.Replace("[PAGE_MODULE]", pageModuleTemplate);
            pnlTemplate.InnerHtml = template;

            if (_id.HasValue)
            {
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