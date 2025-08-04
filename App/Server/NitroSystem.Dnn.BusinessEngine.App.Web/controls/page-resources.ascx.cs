using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.Helpers;
using DotNetNuke.Services.Exceptions;
using System.Linq;
using System.Resources;
using System.Security.AccessControl;
using DotNetNuke.Data;
using System.Data.SqlClient;
using DotNetNuke.Web.Client.ClientResourceManagement;
using NitroSystem.Dnn.BusinessEngine.Core.ADO_NET;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.App.Web.Dto;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
	public partial class PageResources : UserControl
	{
		#region Properties

		public string PortalAlias { get; set; }

		public int DnnTabId { get; set; }

		public int DnnUserId { get; set; }

		public string DnnUserDisplayName { get; set; }

		public Guid? ModuleGuid { get; set; }

		public string ModuleName { get; set; }

		public bool IsPanel { get; set; }

		public bool IsModuleInAllTabs { get; set; }

		public Control PanelResourcesControl { get; set; }

		public string SiteRoot
		{
			get
			{
				string domainName = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Context.Request)) + "/";
				return domainName;
			}
		}

		public string ApiBaseUrl
		{
			get
			{
				string domainName = DotNetNuke.Common.Globals.GetPortalDomainName(this.PortalAlias, Request, true);
				return domainName + "/DesktopModules/";
			}
		}

		public string Version
		{
			get
			{
				return Host.CrmVersion.ToString();
			}
		}

		public bool IsRegisteredPageResources
		{
			get
			{
				return this.Page.Header.FindControl("bEngine_PageResources") != null;
			}
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		internal void RegisterPageResources()
		{
			try
			{
				if (this.ModuleGuid != null)
				{
					if (this.Page.Header.FindControl("bEngine_baseScript") == null)
					{
						var baseScript = new LiteralControl(@"
                            <script type=""text/javascript"">
								const ComponentRegistry = {
									controllers: {},

									register: function (type, controllerClass) {
										this.controllers[type] = controllerClass;
									},

									resolve: function (type) {
										return this.controllers[type];
									}
								};

								const ActionRegistry = {
									controllers: {},

									register: function (type, controllerClass) {
										this.controllers[type] = controllerClass;
									},

									resolve: function (type) {
										return this.controllers[type];
									}
								};

                                window.bEngineGlobalSettings = {
                                    siteRoot: '" + this.SiteRoot + @"',
                                    apiBaseUrl: '" + this.ApiBaseUrl + @"',
                                    modulePath: '/DesktopModules/BusinessEngine/',
                                    userId: " + this.DnnUserId + @",
                                    userDisplayName: '" + this.DnnUserDisplayName + @"',
                                    version: '" + this.Version + @"',
                                    debugMode: false
                                };
                            </script>"
							);

						baseScript.ID = "bEngine_baseScript";
						this.Page.Header.Controls.Add(baseScript);
					}

					List<string> registeredResources = new List<string>();

					int type = !this.IsPanel ? 1 : 2;
					int dnnPageId = type == 1 ? this.DnnTabId : -1;
					Guid? moduleId = type == 2 ? this.ModuleGuid : null;

					var sqlReader = new SqlQueryExecutor();
					var resources = sqlReader.ExecuteQuery<PageResourceDto>(
						"BusinessEngine_GetPageResources",
						System.Data.CommandType.StoredProcedure,
						new Dictionary<string, object>
						{
							{ "@Type", type } ,
							{ "@PageId", dnnPageId },
							{ "@ModuleId", moduleId }
						}
					);

					if (!this.IsRegisteredPageResources)
					{
						foreach (var item in resources)
						{
							registeredResources.Add(item.ResourcePath);

							RegisterPageResources(item.ResourceType, item.ResourcePath, item.LoadOrder);
						}

						this.Page.Header.Controls.Add(new LiteralControl(@"<span id=""bEngine_PageResources""><!--business engine registered resources--></span>"));
					}

					//if (this.IsModuleInAllTabs && this.ModuleGuid != null)
					//{
					//    var moduleResources = await moduleService.GetPageResourcesByModuleViewModelAsync(this.ModuleGuid.Value);
					//    foreach (var item in moduleResources.Where(r => r.IsActive && registeredResources.Contains(r.ResourcePath) == false))
					//    {
					//        RegisterPageResources(item.ResourceType, item.ResourcePath, item.LoadOrder);
					//    }
					//}
				}
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		private void RegisterPageResources(string resourceType, string resourcePath, int priority)
		{
			if (this.IsPanel && this.PanelResourcesControl != null)
			{
				if (resourceType == "css")
				{
					bool notFound = true;

					if (1 == 1 || CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
					{
						string rtlFilePath = string.Empty;
						var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resourcePath);
						if (!string.IsNullOrEmpty(fileNameWithoutExtension) && fileNameWithoutExtension.ToLower().EndsWith(".min"))
							rtlFilePath = Path.GetDirectoryName(resourcePath) + @"\" + Path.GetFileNameWithoutExtension(fileNameWithoutExtension) + ".rtl.min" + Path.GetExtension(resourcePath);
						else
							rtlFilePath = Path.GetDirectoryName(resourcePath) + @"\" +
								Path.GetFileNameWithoutExtension(resourcePath) + ".rtl" + Path.GetExtension(resourcePath);

						if (File.Exists(MapPath(rtlFilePath)))
						{
							Core.Infrastructure.ClientResources.ClientResourceManager.RegisterStyleSheet(this.PanelResourcesControl, rtlFilePath, this.Version);
							notFound = false;
						}
					}

					if (notFound) Core.Infrastructure.ClientResources.ClientResourceManager.RegisterStyleSheet(this.PanelResourcesControl, resourcePath, this.Version);

				}
				if (resourceType == "js")
					Core.Infrastructure.ClientResources.ClientResourceManager.RegisterScript(this.PanelResourcesControl, resourcePath, this.Version);
			}
			else
			{
				if (resourceType == "css")
					ClientResourceManager.RegisterStyleSheet(base.Page, resourcePath, priority);
				if (resourceType == "js")
					ClientResourceManager.RegisterScript(base.Page, resourcePath, priority);
			}
		}
	}
}