using System.Text;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes
{
    public abstract class Node
    {
        public abstract void Render(StringBuilder sb, TemplateContext context);
    }
}