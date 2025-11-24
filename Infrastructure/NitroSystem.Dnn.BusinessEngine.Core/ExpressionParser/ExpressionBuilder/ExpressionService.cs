using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder
{
    public sealed class ExpressionService : IExpressionService
    {
        // فقط TypeDescriptorها رو نگه می‌داریم، نه کل Expressionها
        private readonly ConcurrentDictionary<Type, TypeDescriptor> _descriptors =
            new ConcurrentDictionary<Type, TypeDescriptor>();

        // حذف _compiledCache چون typeها پویا و متغیرند
        public object Evaluate(string expression, ConcurrentDictionary<string, object> data)
        {
            // هر بار کامپایل مجدد، اما سبک و امن
            var param = Expression.Parameter(typeof(ConcurrentDictionary<string, object>), "module");
            var body = BuildExpression(param, expression, data);
            var lambda = Expression.Lambda<Func<ConcurrentDictionary<string, object>, object>>(body, param);
            var func = lambda.Compile();

            return func(data);
        }

        public T Evaluate<T>(string expression, ConcurrentDictionary<string, object> data)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return default;

            var result = Evaluate(expression, data);
            if (result is T typed)
                return typed;

            // تلاش برای تبدیل داینامیک در صورت تفاوت type
            try
            {
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        //public T Evaluate<T>(string expression, ConcurrentDictionary<string, object> data)
        //{
        //    if (!string.IsNullOrWhiteSpace(expression))
        //    {
        //        var result = Evaluate(expression, data);
        //        if (result is T typed)
        //            return typed;
        //    }

        //    return default(T);
        //}

        // BuildDataSetter: کامپایل به اکشن
        public Action<object> BuildDataSetter(string path, ConcurrentDictionary<string, object> data)
        {
            var valueParam = Expression.Parameter(typeof(object), "value");
            var (targetExpr, propInfo) = BuildPropertyAccess(path, data, forSetter: true, valueParam);

            if (propInfo == null)
            {
                return Expression.Lambda<Action<object>>(targetExpr, valueParam).Compile();
            }
            else
            {
                var assign = Expression.Assign(targetExpr, Expression.Convert(valueParam, propInfo.PropertyType));
                return Expression.Lambda<Action<object>>(assign, valueParam).Compile();
            }
        }

        // ساخت Expression از رشته
        private Expression BuildExpression1(ParameterExpression moduleParam, string expr, ConcurrentDictionary<string, object> data)
        {
            // literal
            if (TryParseLiteral(expr, out var lit))
                return Expression.Constant(lit, typeof(object));

            // function
            if (IsFunction(expr, out string funcName, out string[] args))
            {
                if (!ExpressionFunctions.BuiltIn.TryGetValue(funcName, out var fn))
                    throw new InvalidOperationException($"Unknown function {funcName}");

                var fnConst = Expression.Constant(fn);

                // آرگومان‌ها
                var argExprs = args.Select(a => BuildExpression(moduleParam, a, data)).ToArray();

                return Expression.Invoke(fnConst, argExprs);
            }

            // path
            var (targetExpr, propInfo) = BuildPropertyAccess(expr, data, forSetter: false);

            return Expression.Convert(targetExpr, typeof(object));
        }
        private Expression BuildExpression(ParameterExpression moduleParam, string expr, ConcurrentDictionary<string, object> data)
        {
            expr = expr.Trim();

            // 🔸 اگر پرانتز یا عملگرهای منطقی وجود دارند، parser را فعال کن
            if (expr.Contains("==") || expr.Contains("!=") || expr.Contains(">") || expr.Contains("<") ||
                expr.Contains("&&") || expr.Contains("||") || expr.Contains("("))
            {
                var tokens = Tokenize(expr).ToList();
                int index = 0;
                var parsed = ParseExpression(tokens, ref index, moduleParam, data);
                return Expression.Convert(parsed, typeof(object));
            }

            // 🔸 literal
            if (TryParseLiteral(expr, out var lit))
                return Expression.Constant(lit, typeof(object));

            // 🔸 function
            if (IsFunction(expr, out string funcName, out string[] args))
            {
                if (!ExpressionFunctions.BuiltIn.TryGetValue(funcName, out var fn))
                    throw new InvalidOperationException($"Unknown function {funcName}");

                var fnConst = Expression.Constant(fn);
                var argExprs = args.Select(a => BuildExpression(moduleParam, a, data)).ToArray();
                return Expression.Invoke(fnConst, argExprs);
            }

            // 🔸 property path
            var (targetExpr, propInfo) = BuildPropertyAccess(expr, data, forSetter: false);
            return Expression.Convert(targetExpr, typeof(object));
        }

        // دسترسی به property chain
        private (Expression targetExpr, PropertyInfo lastProp) BuildPropertyAccess(
            string path,
            ConcurrentDictionary<string, object> data,
            bool forSetter,
            ParameterExpression valueParam = null)
        {
            var parts = path.Split('.');

            if (parts.Length == 1)
            {
                var keyExpr = Expression.Constant(parts[0]);
                var dictExpr = Expression.Constant(data);

                if (forSetter)
                {
                    var setMethod = typeof(ConcurrentDictionary<string, object>).GetMethod("set_Item");
                    var callExpr = Expression.Call(dictExpr, setMethod, keyExpr, valueParam);
                    return (callExpr, null);
                }
                else
                {
                    var getMethod = typeof(ConcurrentDictionary<string, object>).GetMethod("get_Item");
                    var getExpr = Expression.Call(dictExpr, getMethod, keyExpr);
                    return (getExpr, null);
                }
            }

            Expression expr = Expression.Convert(
                Expression.Call(
                    Expression.Constant(data),
                    typeof(ConcurrentDictionary<string, object>).GetMethod("get_Item"),
                    Expression.Constant(parts[0])
                ),
                typeof(object)
            );

            PropertyInfo lastProp = null;

            for (int i = 1; i < parts.Length; i++)
            {
                if (expr.Type == typeof(object))
                {
                    var baseType = data[parts[0]]?.GetType();

                    expr = Expression.Convert(expr, baseType);
                }

                var currentVar = Expression.Variable(expr.Type, "cur");
                var assign = Expression.Assign(currentVar, expr);

                var nullCheck = Expression.Equal(currentVar, Expression.Constant(null, expr.Type));

                var safeCur = Expression.Condition(
                    nullCheck,
                    Expression.Default(expr.Type),
                    currentVar
                );

                expr = Expression.Block(new[] { currentVar }, assign, safeCur);

                var dictType = expr.Type;
                if (typeof(IDictionary).IsAssignableFrom(dictType))
                {
                    var keyExpr = Expression.Constant(parts[i], typeof(string));
                    var tryGetValue = dictType.GetMethod("TryGetValue");
                    if (tryGetValue != null)
                    {
                        // گرفتن نوع Value دیکشنری
                        var valueType = dictType.IsGenericType
                            ? dictType.GetGenericArguments()[1]   // دومین generic arg
                            : typeof(object);

                        var valueVar = Expression.Variable(valueType, "val");

                        expr = Expression.Block(
                            new[] { valueVar },
                            Expression.Condition(
                                Expression.Call(expr, tryGetValue, keyExpr, valueVar),
                                Expression.Convert(valueVar, typeof(object)),  // به object تبدیل می‌کنیم
                                Expression.Constant(null, typeof(object))
                            )
                        );
                        continue;
                    }
                }

                // اگر کلاس معمولی باشه → پراپرتی
                var propInfo = expr.Type.GetProperty(parts[i]);
                if (propInfo == null)
                {
                    expr = Expression.Constant(null, typeof(object));
                    break;
                }

                lastProp = propInfo;
                expr = Expression.Property(expr, propInfo);
            }

            return (expr, lastProp);
        }

        private (Expression targetExpr, PropertyInfo lastProp) BuildPropertyAccess2(
    string path,
    ConcurrentDictionary<string, object> data,
    bool forSetter,
    ParameterExpression valueParam = null)
        {
            var parts = path.Split('.');
            var dictExpr = Expression.Constant(data);

            // فقط کلید اول (Test)
            var keyExpr = Expression.Constant(parts[0]);
            var tryGetValue = typeof(ConcurrentDictionary<string, object>).GetMethod("TryGetValue");

            var valueVar = Expression.Variable(typeof(object), "val");
            var callTryGet = Expression.Call(dictExpr, tryGetValue, keyExpr, valueVar);

            // اگر کلید وجود نداشت → null
            var assign = Expression.Condition(
                callTryGet,
                valueVar,
                Expression.Constant(null, typeof(object))
            );

            Expression expr = Expression.Block(new[] { valueVar }, assign);

            PropertyInfo lastProp = null;

            // ادامه‌ی chain (مثل Test.Name.LastName)
            for (int i = 1; i < parts.Length; i++)
            {
                expr = Expression.Convert(expr, typeof(object));

                var propInfo = expr.Type.GetProperty(parts[i]);
                if (propInfo == null)
                {
                    expr = Expression.Constant(null, typeof(object));
                    break;
                }

                lastProp = propInfo;
                expr = Expression.Property(Expression.Convert(expr, propInfo.DeclaringType), propInfo);
            }

            // Setter mode
            if (forSetter)
            {
                var setMethod = typeof(ConcurrentDictionary<string, object>).GetMethod("set_Item");
                var assignSet = Expression.Call(dictExpr, setMethod, keyExpr, valueParam);
                return (assignSet, lastProp);
            }

            return (expr, lastProp);
        }

        private TypeDescriptor GetDescriptor(Type type)
            => _descriptors.GetOrAdd(type, t => new TypeDescriptor(t));

        private bool IsFunction(string expr, out string funcName, out string[] args)
        {
            funcName = null;
            args = null;

            var idx = expr.IndexOf('(');
            if (idx > 0 && expr.EndsWith(")"))
            {
                funcName = expr.Substring(0, idx);
                var inner = expr.Substring(idx + 1, expr.Length - idx - 2);

                // جدا کردن پارامترها → Course.Name, 2, 5
                args = SplitArgs(inner).ToArray();
                return true;
            }

            return false;
        }

        private IEnumerable<string> SplitArgs(string input)
        {
            // خیلی ساده: با کاما جدا می‌کنیم (می‌شه بعداً tokenizer قوی‌تر نوشت)
            return input.Split(',').Select(x => x.Trim());
        }

        private bool TryParseLiteral(string expr, out object value)
        {
            //// 1️⃣ اگر مسیر خالی یا null است
            //if (string.IsNullOrWhiteSpace(path))
            //    return (Expression.Constant(null, typeof(object)), null);

            //// 2️⃣ اگر مسیر literal مثل "null" یا "true"/"false" است
            //if (path.Equals("null", StringComparison.OrdinalIgnoreCase))
            //    return (Expression.Constant(null, typeof(object)), null);

            //if (bool.TryParse(path, out var boolVal))
            //    return (Expression.Constant(boolVal, typeof(object)), null);

            //if (decimal.TryParse(path, out var numVal))
            //    return (Expression.Constant(numVal, typeof(object)), null);

            //// 3️⃣ اگر مسیر رشته (درون کوتیشن) است
            //if (path.StartsWith("\"") && path.EndsWith("\""))
            //    return (Expression.Constant(path.Trim('"'), typeof(object)), null);

            if (expr.Equals("null", StringComparison.OrdinalIgnoreCase)) { value = null; return true; }
            if (int.TryParse(expr, out var i)) { value = i; return true; }
            if (double.TryParse(expr, out var d)) { value = d; return true; }
            if (bool.TryParse(expr, out var b)) { value = b; return true; }
            if (expr.StartsWith("\"") && expr.EndsWith("\"")) { value = expr.Trim('"'); return true; }

            value = null;
            return false;
        }

        // ===================== Tokenizer =====================
        private IEnumerable<string> Tokenize(string expr)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];

                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                if ("()=!<>|&".Contains(c))
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }

                    // دو کاراکتری‌ها مثل "==", "!=", ">=", "<=", "&&", "||"
                    if (i + 1 < expr.Length)
                    {
                        string two = expr.Substring(i, 2);
                        if (new[] { "==", "!=", ">=", "<=", "&&", "||" }.Contains(two))
                        {
                            tokens.Add(two);
                            i++;
                            continue;
                        }
                    }

                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }

        // ===================== Parser =====================
        private Expression ParseExpression(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
        {
            // OR level
            var left = ParseAnd(tokens, ref index, moduleParam, data);
            while (index < tokens.Count && tokens[index] == "||")
            {
                index++;
                var right = ParseAnd(tokens, ref index, moduleParam, data);
                left = Expression.OrElse(ToBool(left), ToBool(right));
            }
            return left;
        }

        private Expression ParseAnd(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
        {
            // AND level
            var left = ParseComparison(tokens, ref index, moduleParam, data);
            while (index < tokens.Count && tokens[index] == "&&")
            {
                index++;
                var right = ParseComparison(tokens, ref index, moduleParam, data);
                left = Expression.AndAlso(ToBool(left), ToBool(right));
            }
            return left;
        }

        private Expression ParseComparison(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
        {
            var left = ParsePrimary(tokens, ref index, moduleParam, data);

            if (index < tokens.Count)
            {
                string op = tokens[index];
                if (new[] { "==", "!=", ">", "<", ">=", "<=" }.Contains(op))
                {
                    index++;
                    var right = ParsePrimary(tokens, ref index, moduleParam, data);
                    return BuildBinaryComparison(left, right, op);
                }
            }

            return left;
        }

        private Expression ParsePrimary(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
        {
            if (index >= tokens.Count)
                return Expression.Constant(null, typeof(object));

            string token = tokens[index++];

            // 🔹 پرانتز باز
            if (token == "(")
            {
                var inner = ParseExpression(tokens, ref index, moduleParam, data);
                if (index < tokens.Count && tokens[index] == ")")
                    index++;
                return inner;
            }

            // 🔹 NOT منطقی
            if (token == "!")
            {
                var operand = ParsePrimary(tokens, ref index, moduleParam, data);
                return Expression.Not(ToBool(operand));
            }

            // 🔹 literal
            if (TryParseLiteral(token, out var lit))
                return Expression.Constant(lit, typeof(object));

            // 🔹 property / path
            var (target, _) = BuildPropertyAccess(token, data, false);
            return Expression.Convert(target, typeof(object));
        }

        // کمکی برای boolean conversion
        private Expression ToBool(Expression expr)
        {
            if (expr.Type == typeof(bool))
                return expr;

            return Expression.NotEqual(expr, Expression.Constant(null, typeof(object)));
        }

        // ساخت مقایسه دو operand
        private Expression BuildBinaryComparison(Expression left, Expression right, string op)
        {
            left = Expression.Convert(left, typeof(object));
            right = Expression.Convert(right, typeof(object));

            switch (op)
            {
                case "==": return Expression.Equal(left, right);
                case "!=": return Expression.NotEqual(left, right);
                case ">": return Expression.GreaterThan(left, right);
                case "<": return Expression.LessThan(left, right);
                case ">=": return Expression.GreaterThanOrEqual(left, right);
                case "<=": return Expression.LessThanOrEqual(left, right);
                default: return Expression.Constant(false);
            }
        }
    }
}