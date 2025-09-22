using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using NitroSystem.Dnn.BusinessEngine.App.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Services.ModuleData;
using NitroSystem.Dnn.BusinessEngine.App.Web.Mvc.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.IO;
using NitroSystem.Dnn.BusinessEngine.Core.ADO_NET;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Asynchronous;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Mvc.Controllers
{
    public class ModuleController : DnnController
    {
        private readonly IUserDataStore _userDataStore;
        private readonly IModuleService _moduleService;
        private readonly IActionService _actionService;
        private readonly IActionWorker _actionWorker;

        public ModuleController(
            IUserDataStore userDataStore,
            IModuleService moduleService,
            IActionService actionService,
            IActionWorker actionWorker
        )
        {
            _userDataStore = userDataStore;
            _moduleService = moduleService;
            _actionService = actionService;
            _actionWorker = actionWorker;
        }

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

        private bool IsRegisteredPageResources
        {
            get
            {
                return DnnPage.Header.FindControl("b-page-resources") != null;
            }
        }

        //private string _studioUrl
        //{
        //    get
        //    {
        //        string moduleParam = _id.HasValue ? "id=" + _id.ToString() : "d=" + ModuleContext.ModuleId.ToString();

        //        return ResolveUrl(string.Format("~/DesktopModules/BusinessEngine/studio.aspx?s={0}{1}&m=create-module&{2}&ru={3}", _scenarioName, _siteRoot, moduleParam, TabId));
        //    }
        //}

        #endregion

        public ActionResult Index()
        {
            var model = new ModuleView() { Data = new Models.ModuleData() };

            var m = GetModuleLiteData();

            if (m != null)
            {
                _id = m.Id;
                _scenarioName = m.ScenarioName;
                _moduleName = m.ModuleName;

                model.ConnectionId = Guid.NewGuid().ToString();
                model.Id = _id.Value;
                model.RtlCssClass = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? "b--rtl" : "";

                var module = _moduleService.GetModuleViewModel(model.Id);

                var moduleData = _userDataStore.GetOrCreateModuleData(model.ConnectionId, module.Id, PortalSettings);
                moduleData["_PageParam"] = Shared.Helpers.Globals.UrlHelper.ParsePageParameters(DnnPage.Request.Url.OriginalString);

                _actionWorker.CallActions(moduleData, model.Id, null, "OnPageLoad", PortalSettings);

                var data = _userDataStore.GetDataForClients(model.Id, moduleData);
                var variables = _moduleService.GetModuleVariables(model.Id, Services.Enums.ModuleVariableScope.Global);
                var fields = _moduleService.GetFieldsViewModel(model.Id);
                var actions = _actionService.GetActionsDtoForClient(model.Id);

                model.Data.data = data;
                model.Data.variables = variables;
                model.Data.fields = fields;
                model.Data.actions = actions;

                var template = GetTemplate();
                model.Template = template;

                RegisterPageResources();
            }

            return View(model);
        }

        private ModuleLiteData GetModuleLiteData()
        {
            var cacheKey = "BE_Modules_" + ModuleContext.ModuleId;
            var module = DataCache.GetCache<ModuleLiteData>(cacheKey);

            if (module == null)
            {
                using (var connection = new SqlConnection(DataProvider.Instance().ConnectionString))
                {
                    connection.Open();

                    using (var command = new SqlCommand("dbo.BusinessEngine_GetModuleLite", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@DnnModuleId", ModuleContext.ModuleId);

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

        private string GetTemplate()
        {
            var cacheKey = "BE_Modules_Template" + ModuleContext.ModuleId;
            var template = DataCache.GetCache<string>(cacheKey);

            if (string.IsNullOrEmpty(template))
            {
                string modulePath = PortalSettings.HomeSystemDirectory + @"business-engine/";
                string moduleTemplateUrl = string.Format("{0}/{1}/{2}/module--{2}.html", modulePath, _scenarioName, _moduleName);
                template = FileUtil.GetFileContent(HttpContext.Server.MapPath(moduleTemplateUrl));

                DataCache.SetCache(cacheKey, template);
            }

            return template;
        }

        private void RegisterPageResources()
        {
            if (DnnPage.Header.FindControl("b-baseScript") == null)
            {
                var baseScript = new LiteralControl(@"
                            <script type=""text/javascript"">
								window.ComponentRegistry = {
									controllers: {},

									register: function (type, controllerClass) {
										this.controllers[type] = controllerClass;
									},

									resolve: function (type) {
										return this.controllers[type];
									}
								};

								window.ActionRegistry = {
									controllers: {},

									register: function (type, controllerClass) {
										this.controllers[type] = controllerClass;
									},

									resolve: function (type) {
										return this.controllers[type];
									}
								};
                            </script>"
                    );

                baseScript.ID = "b-baseScript";
                DnnPage.Header.Controls.Add(baseScript);
            }

            List<string> registeredResources = new List<string>();

            var sqlReader = new SqlQueryExecutor();
            var resources = sqlReader.ExecuteQuery<PageResourceDto>(
                "BusinessEngine_GetPageResources",
                System.Data.CommandType.StoredProcedure,
                new Dictionary<string, object>
                {
                            { "@Type", 1 } ,
                            { "@PageId",  ModuleContext.TabId },
                            { "@ModuleId",_id  }
                }
            );

            if (!IsRegisteredPageResources)
            {
                foreach (var item in resources)
                {
                    registeredResources.Add(item.ResourcePath);

                    RegisterPageResources(item.ResourceType, item.ResourcePath, item.LoadOrder);
                }

                DnnPage.Header.Controls.Add(new LiteralControl(@"<span id=""b-page-resources""><!--business engine registered resources--></span>"));

            }
        }

        private void RegisterPageResources(string resourceType, string resourcePath, int priority)
        {
            if (resourceType == "css")
                ClientResourceManager.RegisterStyleSheet(DnnPage, resourcePath, priority);
            if (resourceType == "js")
                ClientResourceManager.RegisterScript(DnnPage, resourcePath, priority);
        }

    }
}
