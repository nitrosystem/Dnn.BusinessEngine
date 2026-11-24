using System.Text;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes
{
    public class RootNode : Node
    {
        public List<Node> Children { get; } = new();

        public override void Render(StringBuilder sb, TemplateContext context)
        {
            foreach (var child in Children)
                child.Render(sb, context);
        }
    }
}