using System.Text;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes
{
    public class TextNode : Node
    {
        public string Text { get; }
        public TextNode(string text) => Text = text;

        public override void Render(StringBuilder sb, TemplateContext context)
        {
            sb.Append(ExpressionInterpolator.Interpolate(Text, context));
        }
    }
}