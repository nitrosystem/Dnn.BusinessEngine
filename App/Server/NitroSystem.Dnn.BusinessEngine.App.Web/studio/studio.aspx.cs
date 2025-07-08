using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ClientResources;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using System.Web.Helpers;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Web
{
    public partial class Studio : Page
    {
        #region Properties

        public string ScenarioNameParam
        {
            get
            {
                return Request.QueryString["s"];
            }
        }

        public string PortalIdParam
        {
            get
            {
                return Request.QueryString["p"];
            }
        }

        public string PortalAliasIdParam
        {
            get
            {
                return Request.QueryString["a"];
            }
        }

        public string DnnModuleIdParam
        {
            get
            {
                return Request.QueryString["d"];
            }
        }

        public string ModuleIdParam
        {
            get
            {
                return Request.QueryString["id"];
            }
        }

        public string ModuleTypeParam
        {
            get
            {
                return Request.QueryString["m"];
            }
        }

        public UserInfo UserInfo
        {
            get
            {
                return UserController.Instance.GetCurrentUserInfo();
            }
        }

        public int UserId
        {
            get
            {
                return this.UserInfo.UserID;
            }
        }

        public Guid ScenarioId { get; set; }

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
                PortalAliasInfo objPortalAliasInfo = PortalAliasController.Instance.GetPortalAliasByPortalAliasID(int.Parse(this.PortalAliasIdParam));

                string domainName = DotNetNuke.Common.Globals.GetPortalDomainName(objPortalAliasInfo.HTTPAlias, Request, true);
                return domainName + "/DesktopModules/";
            }
        }

        public string ConnectionId
        {
            get
            {
                return Request.AnonymousID;
            }
        }

        public string Version
        {
            get
            {
                return Host.CrmVersion.ToString();
            }
        }

        #endregion

        #region EventHandler
        protected void Page_Init(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));
        }

        protected  void Page_Load(object sender, EventArgs e)
        {
            if (this.UserInfo.UserID == -1)
                Response.Redirect(DotNetNuke.Common.Globals.LoginURL(HttpUtility.UrlEncode(this.Request.Url.PathAndQuery), true));

            if (!this.UserInfo.IsInRole("Administrators"))
                Response.Redirect(DotNetNuke.Common.Globals.AccessDeniedURL());

            ProcessData();
        }

        #endregion  

        private void ProcessData()
        {
            using (var connection = new SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();

                // خواندن ScenarioID با استفاده از پارامترهای امن
                using (var command = new SqlCommand("SELECT Id FROM dbo.BusinessEngine_Scenarios WHERE ScenarioName = @ScenarioName", connection))
                {
                    command.Parameters.AddWithValue("@ScenarioName", this.ScenarioNameParam);

                    var scenarioId = command.ExecuteScalar();
                    this.ScenarioId = scenarioId != DBNull.Value && scenarioId != null ? (Guid)scenarioId : Guid.Empty;
                }

                // خواندن منابع استودیو از استورپروسیجر
                using (var command = new SqlCommand("dbo.BusinessEngine_GetStudioResources", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // در صورت نیاز به پارامتر ورودی، از این خط استفاده کن:
                    // command.Parameters.AddWithValue("@SomeParameter", someValue);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string resourcePath = reader["ResourcePath"] as string ?? string.Empty;
                            string resourceType = reader["ResourceType"] as string ?? string.Empty;

                            if (resourceType.Equals("css", StringComparison.OrdinalIgnoreCase))
                                ClientResourceManager.RegisterStyleSheet(pnlResources, resourcePath, this.Version);

                            if (resourceType.Equals("js", StringComparison.OrdinalIgnoreCase))
                                ClientResourceManager.RegisterScript(pnlResources, resourcePath, this.Version);
                        }
                    }
                }

                connection.Close();
            }
        }
    }
}