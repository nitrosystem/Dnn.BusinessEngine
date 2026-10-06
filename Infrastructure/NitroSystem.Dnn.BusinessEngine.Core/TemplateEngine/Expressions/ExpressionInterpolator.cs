using System.Text.RegularExpressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Expressions
{
    public static class ExpressionInterpolator
    {
        public static string Interpolate(string text, TemplateContext context)
        {
            return Regex.Replace(text, @"#\{\{(.+?)\}\}", m =>
            {
                var expr = m.Groups[1].Value.Trim();
                return ExpressionEvaluator.Resolve(expr, context)?.ToString() ?? "";
            });
        }
    }
}