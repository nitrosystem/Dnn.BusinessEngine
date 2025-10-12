using System;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Client.ClientResourceManagement;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.Web.Models;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public partial class PageResources : UserControl
    {
        #region Properties

        public Guid? ModuleId { get; set; }

        public string ModuleName { get; set; }

        public int DnnTabId { get; set; }

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
                return this.Page.Header.FindControl("b-page-resources") != null;
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
                if (this.ModuleId != null)
                {
                    if (this.Page.Header.FindControl("b-baseScript") == null)
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
                            </script>"
                            );

                        baseScript.ID = "b-baseScript";
                        this.Page.Header.Controls.Add(baseScript);
                    }

                    List<string> registeredResources = new List<string>();

                    var resources = ExecuteQuery<ModuleOutputResourceDto>("dbo.BusinessEngine_App_GetModuleOutputResources", CommandType.StoredProcedure,
                        new Dictionary<string, object>
                        {
                            { "@Type", 1 } ,
                            { "@PageId",  this.DnnTabId },
                            { "@ModuleId", this.ModuleId }
                        }
                    );

                    if (!this.IsRegisteredPageResources)
                    {
                        foreach (var item in resources)
                        {
                            registeredResources.Add(item.ResourcePath);

                            RegisterPageResources(item.ResourceContentType, item.ResourcePath, item.LoadOrder);
                        }

                        this.Page.Header.Controls.Add(new LiteralControl(@"<span id=""b-page-resources""><!--business engine registered resources--></span>"));
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

        private void RegisterPageResources(ModuleResourceContentType resourceType, string resourcePath, int priority)
        {
            //if (this.IsPanel && this.PanelResourcesControl != null)
            //{
            //	if (resourceType == "css")
            //	{
            //		bool notFound = true;

            //		if (1 == 1 || CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
            //		{
            //			string rtlFilePath = string.Empty;
            //			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resourcePath);
            //			if (!string.IsNullOrEmpty(fileNameWithoutExtension) && fileNameWithoutExtension.ToLower().EndsWith(".min"))
            //				rtlFilePath = Path.GetDirectoryName(resourcePath) + @"\" + Path.GetFileNameWithoutExtension(fileNameWithoutExtension) + ".rtl.min" + Path.GetExtension(resourcePath);
            //			else
            //				rtlFilePath = Path.GetDirectoryName(resourcePath) + @"\" +
            //					Path.GetFileNameWithoutExtension(resourcePath) + ".rtl" + Path.GetExtension(resourcePath);

            //			if (File.Exists(MapPath(rtlFilePath)))
            //			{
            //				Core.Infrastructure.ClientResources.ClientResourceManager.RegisterStyleSheet(this.PanelResourcesControl, rtlFilePath, this.Version);
            //				notFound = false;
            //			}
            //		}

            //		if (notFound) Core.Infrastructure.ClientResources.ClientResourceManager.RegisterStyleSheet(this.PanelResourcesControl, resourcePath, this.Version);

            //	}
            //	if (resourceType == "js")
            //		Core.Infrastructure.ClientResources.ClientResourceManager.RegisterScript(this.PanelResourcesControl, resourcePath, this.Version);
            //}
            //else
            //{
            if (resourceType == ModuleResourceContentType.Css)
                ClientResourceManager.RegisterStyleSheet(base.Page, resourcePath, priority);
            if (resourceType == ModuleResourceContentType.Js)
                ClientResourceManager.RegisterScript(base.Page, resourcePath, priority);
            //}
        }

        private static List<T> ExecuteQuery<T>(string queryOrSp, CommandType commandType, Dictionary<string, object> parameters = null) where T : new()
        {
            var result = new List<T>();

            using var connection = new SqlConnection(DataProvider.Instance().ConnectionString);
            using var command = new SqlCommand(queryOrSp, connection)
            {
                CommandType = commandType
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            connection.Open();
            using var reader = command.ExecuteReader();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            while (reader.Read())
            {
                var item = new T();

                foreach (var prop in props)
                {
                    if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull)
                        continue;

                    var value = reader[prop.Name];
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (targetType.IsEnum)
                    {
                        var enumValue = Enum.Parse(targetType, value.ToString());
                        prop.SetValue(item, enumValue);
                    }
                    else
                    {
                        prop.SetValue(item, Convert.ChangeType(value, targetType));
                    }
                }

                result.Add(item);
            }

            return result;
        }

        private string GenerateCacheKey<T>(string queryOrSp, Dictionary<string, object> parameters)
        {
            string paramStr = parameters != null ? string.Join(",", parameters.Select(p => $"{p.Key}={p.Value}")) : "";
            return $"{typeof(T).FullName}_{queryOrSp}_{paramStr}";
        }
    }
}