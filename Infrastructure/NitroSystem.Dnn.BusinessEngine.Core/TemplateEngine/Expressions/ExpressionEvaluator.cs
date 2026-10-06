using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Expressions
{
    public static class ExpressionEvaluator
    {
        // Resolve path (e.c: item.count)
        public static object Resolve(string expr, TemplateContext context)
        {
            var parts = expr.Split('.');
            context.TryGet(parts[0], out var current);

            for (int i = 1; i < parts.Length && current != null; i++)
            {
                var prop = current.GetType().GetProperty(parts[i]);
                current = prop?.GetValue(current);
            }

            return current;
        }

        // Evaluate condition with support for and,or
        public static bool Evaluate(string condition, TemplateContext context)
        {
            // OR lowest precedence
            var orParts = condition.Split(new[] { "or" }, StringSplitOptions.None);

            foreach (var orPart in orParts)
            {
                var andParts = orPart.Split(new[] { "and" }, StringSplitOptions.None);
                bool andResult = true;

                foreach (var andPart in andParts)
                {
                    if (!EvaluateSingle(andPart.Trim(), context))
                    {
                        andResult = false;
                        break;
                    }
                }

                if (andResult) return true; // OR is true, so conditions are correct.
            }

            return false;
        }

        private static bool EvaluateSingle(string expr, TemplateContext context)
        {
            expr = expr.Trim();

            // == 
            if (expr.Contains("=="))
            {
                var parts = expr.Split(new[] { "==" }, 2, StringSplitOptions.None);
                var leftVal = Resolve(parts[0].Trim(), context);
                var rightStr = parts[1].Trim();

                // Convert "true"/"false" to bool
                if (bool.TryParse(rightStr, out var rightBool))
                    return Equals(leftVal, rightBool);

                // Numeric convert
                if (decimal.TryParse(rightStr, out var rightDecimal))
                {
                    if (leftVal != null && decimal.TryParse(leftVal.ToString(), out var leftDecimal))
                        return leftDecimal == rightDecimal;

                    return false;
                }

                // String Comparison
                return leftVal?.ToString() == rightStr.Trim('"');
            }

            // != 
            if (expr.Contains("!="))
            {
                var parts = expr.Split(new[] { "!=" }, 2, StringSplitOptions.None);
                var leftVal = Resolve(parts[0].Trim(), context);
                var rightStr = parts[1].Trim();

                // Convert "true"/"false" to bool
                if (bool.TryParse(rightStr, out var rightBool))
                    return !Equals(leftVal, rightBool);

                // Numeric convert
                if (decimal.TryParse(rightStr, out var rightDecimal))
                {
                    if (leftVal != null && decimal.TryParse(leftVal.ToString(), out var leftDecimal))
                        return leftDecimal != rightDecimal;

                    return false;
                }

                // String Comparison
                return leftVal?.ToString() != rightStr.Trim('"');
            }

            // > 
            if (expr.Contains(">"))
            {
                var parts = expr.Split(new char[] { '>' }, 2); ;
                var left = Convert.ToDecimal(Resolve(parts[0].Trim(), context));
                var right = Convert.ToDecimal(parts[1].Trim());
                return left > right;
            }

            // < 
            if (expr.Contains("<"))
            {
                var parts = expr.Split(new char[] { '<' }, 2); ;
                var left = Convert.ToDecimal(Resolve(parts[0].Trim(), context));
                var right = Convert.ToDecimal(parts[1].Trim());
                return left < right;
            }

            // boolean literal
            if (bool.TryParse(expr, out var boolVal))
                return boolVal;

            // fallback
            var resolved = Resolve(expr, context);
            return resolved != null && (!resolved.Equals(false));
        }
    }
}