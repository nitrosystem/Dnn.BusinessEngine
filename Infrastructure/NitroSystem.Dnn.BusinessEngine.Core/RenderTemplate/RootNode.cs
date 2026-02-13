using System.Collections.Generic;
using System.Text;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public class RootNode : Node
    {
        public List<Node> Children { get; } = new();

        public override void Render(StringBuilder sb, RenderContext context)
        {
            foreach (var child in Children)
                child.Render(sb, context);
        }
    }
}