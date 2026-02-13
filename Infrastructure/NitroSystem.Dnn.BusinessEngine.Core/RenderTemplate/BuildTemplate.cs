using System.Text;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public class BuildTemplate
    {
        public string Render(RootNode root, RenderContext context)
        {
            var sb = new StringBuilder();
            root.Render(sb, context);
            return sb.ToString();
        }
    }
}