using System;
using System.Data;
using System.Web.Helpers;
using System.Web.UI;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Framework;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Dashboard : PortalModuleBase, IActionable
    {
        private readonly ICacheService _cacheService;
        private readonly IExecuteSqlCommand _sqlCommand;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _siteRoot;
        private string _scenarioName;
        private Guid? _id;

        public Dashboard()
        {
            _cacheService = DependencyProvider.GetService<ICacheService>();
            _sqlCommand = DependencyProvider.GetService<IExecuteSqlCommand>();
            _unitOfWork = DependencyProvider.GetService<IUnitOfWork>();

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
                (pageModuleId, pageModuleName) = _cacheService.Get<(Guid?, string)>("Be_Modules_DashboardPage" + pageName);
                if (pageModuleId == null)
                {
                    var commandText = @"
                        SELECT pm.ModuleId,d.ModuleName as ParentModuleName
                        FROM dbo.BusinessEngine_DashboardPageModules pm 
	                        INNER JOIN dbo.BusinessEngine_DashboardPages p on pm.PageId = p.Id
	                        INNER JOIN dbo.BusinessEngineView_Dashboards d on d.id = p.DashboardId
                        WHERE d.SiteModuleId = @SiteModuleId and p.PageName = @PageName";
                    var param = new
                    {
                        SiteModuleId = ModuleId,
                        PageName = pageName
                    };

                    using (var reader = _sqlCommand.ExecuteSqlReader(_unitOfWork, CommandType.Text, commandText, param))
                    {
                        if (reader.Read())
                        {
                            pageModuleId = reader["ModuleId"] as Guid?;
                            pageModuleName = reader["ParentModuleName"] as string;
                            _cacheService.Set<(Guid?, string)>("Be_Modules_DashboardPage" + pageName, (pageModuleId, pageModuleName));
                        }
                    }
                }

                if (pageModuleId.HasValue)
                {
                    var parentFolder = StringHelper.ToKebabCase(pageModuleName) + "/";
                    var childTemplates = ModuleService.RenderModule(this.Page, _cacheService, _sqlCommand, _unitOfWork, PortalSettings.HomeSystemDirectory, false, null, ref pageModuleId, out _scenarioName, parentFolder);
                    pageModuleTemplate = childTemplates.Template;
                }
            }

            var templates = ModuleService.RenderModule(this.Page, _cacheService, _sqlCommand, _unitOfWork, PortalSettings.HomeSystemDirectory, true, ModuleId, ref _id, out _scenarioName);
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