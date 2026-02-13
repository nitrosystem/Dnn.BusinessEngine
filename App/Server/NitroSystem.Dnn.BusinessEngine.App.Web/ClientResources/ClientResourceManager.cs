using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace NitroSystem.Dnn.BusinessEngine.Core.ClientResources
{
    public class ClientResourceManager
    {
        public static void RegisterStyleSheet(Control container, string cssFilePath, string version, string media = "all")
        {
            var link = new HtmlLink();
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            link.Href = link.ResolveUrl(cssFilePath) + "?ver=" + version;

            link.Attributes.Add("media", media);

            container.Controls.Add(link);
        }

        public static void RegisterScript(Control container, string scriptFilePath, string version)
        {
            var script = new HtmlGenericControl();
            script.TagName = "script";
            script.Attributes.Add("type", "text/javascript");
            script.Attributes.Add("src", script.ResolveUrl(scriptFilePath) + "?ver=" + version);

            container.Controls.Add(script);
        }
    }
}