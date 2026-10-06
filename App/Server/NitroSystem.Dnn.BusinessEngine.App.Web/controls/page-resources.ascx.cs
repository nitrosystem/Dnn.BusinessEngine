using System;
using System.Web.UI;
using System.Collections.Generic;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Client.ClientResourceManagement;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.ApplicationService.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public partial class PageResources : UserControl
    {
        #region Properties

        public int DnnTabId { get; set; }
        public HashSet<Guid> ModuleIds { get; set; }
        public Control PanelResourcesControl { get; set; }
        public string Version { get { return Host.CrmVersion.ToString(); } }
        public bool IsRegisteredPageResources { get { return this.Page.Header.FindControl("b-page-resources") != null; } }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        internal void RegisterPageResources(IModuleService moduleService)
        {
            try
            {
                if (this.DnnTabId != 0)
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
                            </script>"
                            );

                        baseScript.ID = "b-baseScript";
                        this.Page.Header.Controls.Add(baseScript);
                    }

                    if (!this.IsRegisteredPageResources)
                    {
                        var resources = moduleService.GetModuleOutputResources(DnnTabId, string.Join(",", ModuleIds));
                        foreach (var item in resources)
                        {
                            RegisterPageResources(item.ResourceContentType, item.ResourcePath, item.LoadOrder);
                        }

                        this.Page.Header.Controls.Add(new LiteralControl(@"<span id=""b-page-resources""><!--business engine registered resources--></span>"));
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void RegisterPageResources(ResourceContentType resourceType, string resourcePath, int priority)
        {
            if (resourceType == ResourceContentType.Css)
                ClientResourceManager.RegisterStyleSheet(base.Page, resourcePath, priority);
            if (resourceType == ResourceContentType.Js)
                ClientResourceManager.RegisterScript(base.Page, resourcePath, priority);
        }
    }
}