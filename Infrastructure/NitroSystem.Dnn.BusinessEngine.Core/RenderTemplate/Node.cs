using System.Text;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public abstract class Node
    {
        public abstract void Render(StringBuilder sb, RenderContext context);
    }
}