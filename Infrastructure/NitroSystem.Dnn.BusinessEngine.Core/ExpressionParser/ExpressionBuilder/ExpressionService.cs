using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder
{
    //public sealed class ExpressionService : IExpressionService
    //{
    //    // remove _compiledCache as types are dynamic and variable.
    //    public object Evaluate(string expression, ConcurrentDictionary<string, object> data)
    //    {
    //        //every call recompiled, but lightweight and secure
    //        var param = Expression.Parameter(typeof(ConcurrentDictionary<string, object>), "module");
    //        var body = BuildExpression(param, expression, data);
    //        var lambda = Expression.Lambda<Func<ConcurrentDictionary<string, object>, object>>(body, param);
    //        var func = lambda.Compile();
    //        return func(data);
    //    }

    //    public T Evaluate<T>(string expression, ConcurrentDictionary<string, object> data)
    //    {
    //        if (string.IsNullOrWhiteSpace(expression))
    //            return default;

    //        var result = Evaluate(expression, data);
    //        if (result is T typed)
    //            return typed;

    //        // attemp for dynamic conversion if types differ
    //        try
    //        {
    //            return (T)Convert.ChangeType(result, typeof(T));
    //        }
    //        catch
    //        {
    //            return default;
    //        }
    //    }

    //    public Action<object> BuildDataSetter(string path, ConcurrentDictionary<string, object> data)
    //    {
    //        var valueParam = Expression.Parameter(typeof(object), "value");
    //        var (targetExpr, propInfo) = BuildPropertyAccess(path, data, forSetter: true, valueParam);

    //        if (propInfo == null)
    //        {
    //            return Expression.Lambda<Action<object>>(targetExpr, valueParam).Compile();
    //        }
    //        else
    //        {
    //            var assign = Expression.Assign(targetExpr, Expression.Convert(valueParam, propInfo.PropertyType));
    //            return Expression.Lambda<Action<object>>(assign, valueParam).Compile();
    //        }
    //    }

    //    private Expression BuildExpression(ParameterExpression moduleParam, string expr, ConcurrentDictionary<string, object> data)
    //    {
    //        expr = expr.Trim();

    //        // If parentheses or logical operators are present, enable the parser.
    //        if (expr.Contains("==") || expr.Contains("!=") || expr.Contains(">") || expr.Contains(">=") ||
    //            expr.Contains("<") || expr.Contains("<=") || expr.Contains("&&") || expr.Contains("||"))
    //        {
    //            var tokens = Tokenize(expr).ToList();
    //            int index = 0;
    //            var parsed = ParseExpression(tokens, ref index, moduleParam, data);
    //            return Expression.Convert(parsed, typeof(object));
    //        }

    //        // literal
    //        if (TryParseLiteral(expr, out var lit))
    //            return Expression.Constant(lit, typeof(object));

    //        // function
    //        if (IsFunction(expr, out string funcName, out string[] args))
    //        {
    //            if (!ExpressionFunctions.BuiltIn.TryGetValue(funcName, out var fn))
    //                throw new InvalidOperationException($"Unknown function {funcName}");

    //            var parameters = fn.Method.GetParameters();
    //            if (parameters.Length != args.Length)
    //                throw new InvalidOperationException(
    //                    $"Function '{funcName}' expects {parameters.Length} argument(s), got {args.Length}.");

    //            var fnConst = Expression.Constant(fn);
    //            var argExprs = args
    //                .Select((a, i) => Expression.Convert(BuildExpression(moduleParam, a, data), parameters[i].ParameterType))
    //                .ToArray();

    //            return Expression.Convert(Expression.Invoke(fnConst, argExprs), typeof(object));
    //        }

    //        // property path
    //        var (targetExpr, propInfo) = BuildPropertyAccess(expr, data, forSetter: false);
    //        return Expression.Convert(targetExpr, typeof(object));
    //    }

    //    // Access to property chain
    //    private (Expression targetExpr, PropertyInfo lastProp) BuildPropertyAccess(
    //        string path,
    //        ConcurrentDictionary<string, object> data,
    //        bool forSetter,
    //        ParameterExpression valueParam = null)
    //    {
    //        var parts = path.Split('.');
    //        if (parts.Length == 1)
    //        {
    //            var keyExpr = Expression.Constant(parts[0]);
    //            var dictExpr = Expression.Constant(data);

    //            if (forSetter)
    //            {
    //                var setMethod = typeof(ConcurrentDictionary<string, object>).GetMethod("set_Item");
    //                var callExpr = Expression.Call(dictExpr, setMethod, keyExpr, valueParam);
    //                return (callExpr, null);
    //            }
    //            else
    //            {
    //                var getMethod = typeof(ConcurrentDictionary<string, object>).GetMethod("get_Item");
    //                var getExpr = Expression.Call(dictExpr, getMethod, keyExpr);
    //                return (getExpr, null);
    //            }
    //        }

    //        Expression expr = Expression.Convert(
    //            Expression.Call(
    //                Expression.Constant(data),
    //                typeof(ConcurrentDictionary<string, object>).GetMethod("get_Item"),
    //                Expression.Constant(parts[0])
    //            ),
    //            typeof(object)
    //        );

    //        PropertyInfo lastProp = null;

    //        for (int i = 1; i < parts.Length; i++)
    //        {
    //            if (expr.Type == typeof(object))
    //            {
    //                var baseType = data[parts[0]]?.GetType();

    //                expr = Expression.Convert(expr, baseType);
    //            }

    //            var currentVar = Expression.Variable(expr.Type, "cur");
    //            var assign = Expression.Assign(currentVar, expr);

    //            var nullCheck = Expression.Equal(currentVar, Expression.Constant(null, expr.Type));

    //            var safeCur = Expression.Condition(
    //                nullCheck,
    //                Expression.Default(expr.Type),
    //                currentVar
    //            );

    //            expr = Expression.Block(new[] { currentVar }, assign, safeCur);
    //            var dictType = expr.Type;
    //            if (typeof(IDictionary).IsAssignableFrom(dictType))
    //            {
    //                var keyExpr = Expression.Constant(parts[i], typeof(string));
    //                var tryGetValue = dictType.GetMethod("TryGetValue");
    //                if (tryGetValue != null)
    //                {
    //                    // Get type of Dictionary Value 
    //                    var valueType = dictType.IsGenericType
    //                        ? dictType.GetGenericArguments()[1]   // second generic arg
    //                        : typeof(object);
    //                    var valueVar = Expression.Variable(valueType, "val");

    //                    expr = Expression.Block(
    //                        new[] { valueVar },
    //                        Expression.Condition(
    //                            Expression.Call(expr, tryGetValue, keyExpr, valueVar),
    //                            Expression.Convert(valueVar, typeof(object)),  // Convert to object 
    //                            Expression.Constant(null, typeof(object))
    //                        )
    //                    );
    //                    continue;
    //                }
    //            }

    //            // If class => Property
    //            var propInfo = expr.Type.GetProperty(parts[i]);
    //            if (propInfo == null)
    //            {
    //                expr = Expression.Constant(null, typeof(object));
    //                break;
    //            }

    //            lastProp = propInfo;
    //            expr = Expression.Property(expr, propInfo);
    //        }

    //        return (expr, lastProp);
    //    }

    //    private bool IsFunction(string expr, out string funcName, out string[] args)
    //    {
    //        funcName = null;
    //        args = null;

    //        var idx = expr.IndexOf('(');
    //        if (idx > 0 && expr.EndsWith(")"))
    //        {
    //            funcName = expr.Substring(0, idx);
    //            var inner = expr.Substring(idx + 1, expr.Length - idx - 2);

    //            // Split params => Course.Name, 2, 5
    //            args = SplitArgs(inner).ToArray();
    //            return true;
    //        }

    //        return false;
    //    }

    //    private IEnumerable<string> SplitArgs(string input)
    //    {
    //        //separate with commas
    //        return input.Split(',').Select(x => x.Trim());
    //    }

    //    private bool TryParseLiteral(string expr, out object value)
    //    {
    //        if (expr.Equals("null", StringComparison.OrdinalIgnoreCase)) { value = null; return true; }
    //        if (int.TryParse(expr, out var i)) { value = i; return true; }
    //        if (double.TryParse(expr, out var d)) { value = d; return true; }
    //        if (bool.TryParse(expr, out var b)) { value = b; return true; }
    //        if (expr.StartsWith("\"") && expr.EndsWith("\"")) { value = expr.Trim('"'); return true; }

    //        value = null;
    //        return false;
    //    }

    //    // ===================== Tokenizer =====================
    //    private IEnumerable<string> Tokenize(string expr)
    //    {
    //        var tokens = new List<string>();
    //        var sb = new StringBuilder();
    //        for (int i = 0; i < expr.Length; i++)
    //        {
    //            char c = expr[i];
    //            if (char.IsWhiteSpace(c))
    //            {
    //                if (sb.Length > 0)
    //                {
    //                    tokens.Add(sb.ToString());
    //                    sb.Clear();
    //                }
    //                continue;
    //            }

    //            if ("=!<>|&".Contains(c))
    //            {
    //                if (sb.Length > 0)
    //                {
    //                    tokens.Add(sb.ToString());
    //                    sb.Clear();
    //                }

    //                // two chars like: "==", "!=", ">=", "<=", "&&", "||"
    //                if (i + 1 < expr.Length)
    //                {
    //                    string two = expr.Substring(i, 2);
    //                    if (new[] { "==", "!=", ">=", "<=", "&&", "||" }.Contains(two))
    //                    {
    //                        tokens.Add(two);
    //                        i++;
    //                        continue;
    //                    }
    //                }

    //                tokens.Add(c.ToString());
    //            }
    //            else
    //            {
    //                sb.Append(c);
    //            }
    //        }

    //        if (sb.Length > 0)
    //            tokens.Add(sb.ToString());

    //        return tokens;
    //    }

    //    // ===================== Parser =====================
    //    private Expression ParseExpression(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
    //    {
    //        // OR level
    //        var left = ParseAnd(tokens, ref index, moduleParam, data);
    //        while (index < tokens.Count && tokens[index] == "||")
    //        {
    //            index++;
    //            var right = ParseAnd(tokens, ref index, moduleParam, data);
    //            left = Expression.OrElse(ToBool(left), ToBool(right));
    //        }
    //        return left;
    //    }

    //    private Expression ParseAnd(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
    //    {
    //        // AND level
    //        var left = ParseComparison(tokens, ref index, moduleParam, data);
    //        while (index < tokens.Count && tokens[index] == "&&")
    //        {
    //            index++;
    //            var right = ParseComparison(tokens, ref index, moduleParam, data);
    //            left = Expression.AndAlso(ToBool(left), ToBool(right));
    //        }
    //        return left;
    //    }

    //    private Expression ParseComparison(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
    //    {
    //        var left = ParsePrimary(tokens, ref index, moduleParam, data);

    //        if (index < tokens.Count)
    //        {
    //            string op = tokens[index];
    //            if (new[] { "==", "!=", ">", "<", ">=", "<=" }.Contains(op))
    //            {
    //                index++;
    //                var right = ParsePrimary(tokens, ref index, moduleParam, data);
    //                return BuildBinaryComparison(left, right, op);
    //            }
    //        }

    //        return left;
    //    }

    //    private Expression ParsePrimary(List<string> tokens, ref int index, ParameterExpression moduleParam, ConcurrentDictionary<string, object> data)
    //    {
    //        if (index >= tokens.Count)
    //            return Expression.Constant(null, typeof(object));

    //        string token = tokens[index++];

    //        // opening parenthesis
    //        if (token == "(")
    //        {
    //            var inner = ParseExpression(tokens, ref index, moduleParam, data);
    //            if (index < tokens.Count && tokens[index] == ")")
    //                index++;
    //            return inner;
    //        }

    //        // NOT(logical)
    //        if (token == "!")
    //        {
    //            var operand = ParsePrimary(tokens, ref index, moduleParam, data);
    //            return Expression.Not(ToBool(operand));
    //        }

    //        // literal
    //        if (TryParseLiteral(token, out var lit))
    //            return Expression.Constant(lit, typeof(object));

    //        // property/path
    //        var (target, _) = BuildPropertyAccess(token, data, false);
    //        return Expression.Convert(target, typeof(object));
    //    }

    //    // useful for boolean conversion
    //    private Expression ToBool(Expression expr)
    //    {
    //        if (expr.Type == typeof(bool))
    //            return expr;

    //        return Expression.NotEqual(expr, Expression.Constant(null, typeof(object)));
    //    }

    //    // Comparison operation between two operands
    //    private Expression BuildBinaryComparison(Expression left, Expression right, string op)
    //    {
    //        left = Expression.Convert(left, typeof(object));
    //        right = Expression.Convert(right, typeof(object));

    //        switch (op)
    //        {
    //            case "==": return Expression.Equal(left, right);
    //            case "!=": return Expression.NotEqual(left, right);
    //            case ">": return Expression.GreaterThan(left, right);
    //            case "<": return Expression.LessThan(left, right);
    //            case ">=": return Expression.GreaterThanOrEqual(left, right);
    //            case "<=": return Expression.LessThanOrEqual(left, right);
    //            default: return Expression.Constant(false);
    //        }
    //    }
    //}
}