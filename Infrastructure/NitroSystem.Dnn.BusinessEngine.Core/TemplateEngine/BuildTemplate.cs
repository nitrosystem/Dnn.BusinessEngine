using System.Text;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine
{
    public class BuildTemplate
    {
        public string Render(RootNode root, TemplateContext context)
        {
            var sb = new StringBuilder();
            root.Render(sb, context);
            return sb.ToString();
        }
    }
}