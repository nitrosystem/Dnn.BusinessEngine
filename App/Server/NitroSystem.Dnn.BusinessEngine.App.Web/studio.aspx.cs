using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Helpers;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Shared.Extensions;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public partial class Studio : Page
    {
        public string SiteRoot { get; set; }
        public string ScenarioName { get; set; }
        public Guid? ScenarioId { get; set; }

        public int Version
        {
            get
            {
                return Host.CrmVersion;
            }
        }

        #region EventHandler
        protected void Page_Init(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SiteRoot = Request.QueryString["sr"] ?? "/";
            ScenarioName = Request.QueryString["s"];

            if (!User.Identity.IsAuthenticated)
                Response.Redirect(DotNetNuke.Common.Globals.LoginURL(HttpUtility.UrlEncode(this.Request.Url.PathAndQuery), true));
            else
            {
                var user = UserController.GetUserByName(User.Identity.Name);
                if (!user.IsInRole("Administrators"))
                    Response.Redirect(DotNetNuke.Common.Globals.AccessDeniedURL());
            }

            ProcessData();
        }

        #endregion  

        private void ProcessData()
        {
            var version = Host.CrmVersion.ToString();

            using (var connection = new SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand("SELECT Id FROM dbo.BusinessEngine_Scenarios WHERE ScenarioName = @ScenarioName", connection))
                {
                    command.Parameters.AddWithValue("@ScenarioName", this.ScenarioName ?? string.Empty);
                    this.ScenarioId = command.ExecuteScalar() as Guid?;
                }

                using (var command = new SqlCommand("dbo.BusinessEngine_Studio_GetStudioResources", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var resourcePath = (reader["ResourcePath"] as string ?? string.Empty).ReplaceFrequentTokens();
                            var resourceContentType = reader["ResourceContentType"] as int?;

                            if (resourceContentType == 1)
                                RegisterStyleSheet(pnlResources, resourcePath, version);
                            else if (resourceContentType == 2)
                                RegisterScript(pnlResources, resourcePath, version);
                        }
                    }
                }

                connection.Close();
            }
        }

        private void RegisterStyleSheet(Control container, string cssFilePath, string version, string media = "all")
        {
            var link = new HtmlLink();
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            link.Href = link.ResolveUrl(cssFilePath) + "?ver=" + version;
            link.Attributes.Add("media", media);
            container.Controls.Add(link);
        }

        private void RegisterScript(Control container, string scriptFilePath, string version)
        {
            var script = new HtmlGenericControl();
            script.TagName = "script";
            script.Attributes.Add("type", "text/javascript");
            script.Attributes.Add("src", script.ResolveUrl(scriptFilePath) + "?ver=" + version);
            container.Controls.Add(script);
        }
    }
}