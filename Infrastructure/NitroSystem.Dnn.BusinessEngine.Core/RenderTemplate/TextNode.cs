using System.Text;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public class TextNode : Node
    {
        public string Text { get; }
        public TextNode(string text) => Text = text;

        public override void Render(StringBuilder sb, RenderContext context)
        {
            sb.Append(ExpressionInterpolator.Interpolate(Text, context));
        }
    }
}