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
using System.Threading.Tasks;
using DotNetNuke.Common.Utilities;
using NitroSystem.Dnn.BusinessEngine.App.Web.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Utils;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Module : PortalModuleBase, IActionable
    {
        #region Properties

        public Guid? _id { get; set; }

        public string _scenarioName { get; set; }

        public string _moduleName { get; set; }

        private string _studioUrl
        {
            get
            {
                string moduleParamValue = _id.HasValue ? _id.ToString() : this.ModuleId.ToString();

                return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}&p={1}&a={2}&m=create-module&id={3}&ru={4}", _scenarioName, this.PortalId, this.PortalAlias.PortalAliasID, moduleParamValue, this.TabId));
            }
        }

        #endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            lnkModuleBuilder.Visible = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole("Administrators");
            lnkModuleBuilder.PostBackUrl = _studioUrl;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));

            var module = GetModuleLiteData();

            if (module != null)
            {
                _id = module.Id;
                _scenarioName = module.ScenarioName;
                _moduleName = module.ModuleName;

                var connectionId = Guid.NewGuid();
                var rtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "b--rtl" : "";

                CtlPageResource.ModuleId = module.Id;
                CtlPageResource.ModuleName = module.ModuleName;
                CtlPageResource.DnnTabId = this.TabId;
                CtlPageResource.IsModuleInAllTabs = this.ModuleConfiguration.AllTabs;
                CtlPageResource.RegisterPageResources();

                var templates = GetTemplates();
                pnlTemplate.InnerHtml = $@"
                <div b-controller=""moduleController"" data-module=""{module.Id}"" data-connection=""{connectionId}"" class=""b--module {rtlCssClass}"">
                    {templates.Template}
                </div>";
            }
        }

        #endregion

        private ModuleLiteData GetModuleLiteData()
        {
            var cacheKey = "BE_Modules_" + this.ModuleId;
            var module = DataCache.GetCache<ModuleLiteData>(cacheKey);

            if (module == null)
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
                                module = new ModuleLiteData()
                                {
                                    Id = reader["Id"] as Guid?,
                                    ScenarioName = reader["ScenarioName"] as string,
                                    ModuleName = reader["ModuleName"] as string,
                                    ModuleVersion = reader["ModuleVersion"] as int?
                                };

                                DataCache.SetCache(cacheKey, module);
                            }
                        }

                    }

                    connection.Close();
                }
            }

            return module;
        }

        private (string Preloader, string Template) GetTemplates()
        {
            var cacheKey = "BE_Modules_Template" + this.ModuleId;
            var data = DataCache.GetCache<(string Preloader, string Template)>(cacheKey);
            var preloader = data.Preloader;
            var template = data.Template;

            if (string.IsNullOrEmpty(template))
            {
                string modulePath = this.PortalSettings.HomeSystemDirectory + @"business-engine/";
                string moduleTemplateUrl = string.Format("{0}/{1}/{2}/module--{2}.html", modulePath, _scenarioName, _moduleName);
                template = FileUtil.GetFileContent(MapPath(moduleTemplateUrl));

                data = ("", template);

                DataCache.SetCache(cacheKey, data);
            }

            return data;
        }

        #region IActionable

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(), "Module Builder", "Module.Builder", "", "~/DesktopModules/BusinessEngine/assets/icons/module-builder-16.png", _studioUrl, false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        #endregion
    }
}