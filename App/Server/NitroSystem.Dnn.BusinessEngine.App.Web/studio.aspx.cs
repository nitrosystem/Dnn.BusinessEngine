using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Users;
using NitroSystem.Dnn.BusinessEngine.Core.ClientResources;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Helpers;
using System.Web.UI;

namespace NitroSystem.Dnn.BusinessEngine.App.Web
{
    public partial class Studio : Page
    {
        public string SiteRoot { get; set; }
        public string ScenarioName { get; set; }
        public Guid ScenarioId { get; set; }

        #region EventHandler
        protected void Page_Init(object sender, EventArgs e)
        {
            var code = AntiForgery.GetHtml().ToHtmlString();
            pnlAntiForgery.Controls.Add(new LiteralControl(code));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.SiteRoot = Request.QueryString["sr"];
            this.ScenarioName = Request.QueryString["s"];

            var user = UserController.Instance.GetCurrentUserInfo();

            if (user.UserID == -1)
                Response.Redirect(DotNetNuke.Common.Globals.LoginURL(HttpUtility.UrlEncode(this.Request.Url.PathAndQuery), true));

            if (!user.IsInRole("Administrators"))
                Response.Redirect(DotNetNuke.Common.Globals.AccessDeniedURL());

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

                    this.ScenarioId = (command.ExecuteScalar() as Guid?) ?? Guid.Empty;
                }

                using (var command = new SqlCommand("dbo.BusinessEngine_Studio_GetStudioResources", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var resourcePath = reader["ResourcePath"] as string ?? string.Empty;
                            var resourceContentType = reader["ResourceContentType"] as int?;

                            if (resourceContentType==1)
                                ClientResourceManager.RegisterStyleSheet(pnlResources, resourcePath, version);

                            if (resourceContentType==2)
                                ClientResourceManager.RegisterScript(pnlResources, resourcePath, version);
                        }
                    }
                }

                connection.Close();
            }
        }
    }
}