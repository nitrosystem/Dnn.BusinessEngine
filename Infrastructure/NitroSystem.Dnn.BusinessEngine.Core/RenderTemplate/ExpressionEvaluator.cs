using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public static class ExpressionEvaluator
    {
        // Resolve یک مسیر ساده مثل item.count
        public static object Resolve(string expr, RenderContext context)
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

        // Evaluate شرط با پشتیبانی از && و ||
        public static bool Evaluate(string condition, RenderContext context)
        {
            // OR lowest precedence
            var orParts = condition.Split(new[] { "||" }, StringSplitOptions.None);

            foreach (var orPart in orParts)
            {
                var andParts = orPart.Split(new[] { "&&" }, StringSplitOptions.None);
                bool andResult = true;

                foreach (var andPart in andParts)
                {
                    if (!EvaluateSingle(andPart.Trim(), context))
                    {
                        andResult = false;
                        break;
                    }
                }

                if (andResult) return true; // هر OR درست، کل شرط درست است
            }

            return false;
        }

        private static bool EvaluateSingle(string expr, RenderContext context)
        {
            expr = expr.Trim();

            // == برای مقادیر
            if (expr.Contains("=="))
            {
                var parts = expr.Split(new[] { "==" }, 2, StringSplitOptions.None);
                var leftVal = Resolve(parts[0].Trim(), context);
                var rightStr = parts[1].Trim();

                // تبدیل "true"/"false" به bool
                if (bool.TryParse(rightStr, out var rightBool))
                    return Equals(leftVal, rightBool);

                // تبدیل عددی
                if (decimal.TryParse(rightStr, out var rightDecimal))
                {
                    if (leftVal != null && decimal.TryParse(leftVal.ToString(), out var leftDecimal))
                        return leftDecimal == rightDecimal;
                    return false;
                }

                // مقایسه رشته‌ای
                return leftVal?.ToString() == rightStr.Trim('"');
            }

            // != برای مقادیر
            if (expr.Contains("!="))
            {
                var parts = expr.Split(new[] { "!=" }, 2, StringSplitOptions.None);
                var leftVal = Resolve(parts[0].Trim(), context);
                var rightStr = parts[1].Trim();

                // تبدیل "true"/"false" به bool
                if (bool.TryParse(rightStr, out var rightBool))
                    return !Equals(leftVal, rightBool);

                // تبدیل عددی
                if (decimal.TryParse(rightStr, out var rightDecimal))
                {
                    if (leftVal != null && decimal.TryParse(leftVal.ToString(), out var leftDecimal))
                        return leftDecimal != rightDecimal;
                    return false;
                }

                // مقایسه رشته‌ای
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