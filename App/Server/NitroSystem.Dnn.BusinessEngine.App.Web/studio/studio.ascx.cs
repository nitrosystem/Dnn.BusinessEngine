using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using System;
using System.Globalization;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Web.Modules
{
    public partial class Studio : PortalModuleBase
    {
        public string SiteRoot
        {
            get
            {
                var siteRoot = ServicesFramework.GetServiceFrameworkRoot();
                return siteRoot == "/"
                    ? string.Empty
                    : "sr=" + siteRoot;
            }
        }
    }
}