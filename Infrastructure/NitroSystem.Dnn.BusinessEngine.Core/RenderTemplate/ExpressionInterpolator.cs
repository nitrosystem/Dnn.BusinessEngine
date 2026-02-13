using System.Text.RegularExpressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public static class ExpressionInterpolator
    {
        public static string Interpolate(string text, RenderContext context)
        {
            return Regex.Replace(text, @"#\{\{(.+?)\}\}", m =>
            {
                var expr = m.Groups[1].Value.Trim();
                return ExpressionEvaluator.Resolve(expr, context)?.ToString() ?? "";
            });
        }
    }
}