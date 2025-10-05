using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.TypeLoader;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ExpressionParser.ExpressionBuilder
{
    public sealed class ExpressionService : IExpressionService
    {
        private readonly ConcurrentDictionary<Type, TypeDescriptor> _descriptors =
            new ConcurrentDictionary<Type, TypeDescriptor>();

        private readonly ConcurrentDictionary<string, Delegate> _compiledCache =
            new ConcurrentDictionary<string, Delegate>();

        // Evaluate با کش کردن Expression های کامپایل شده
        public object Evaluate(string expression, ConcurrentDictionary<string, object> data)
        {
            var func = (Func<ConcurrentDictionary<string, object>, object>)_compiledCache.GetOrAdd(expression, expr =>
            {
                var param = Expression.Parameter(typeof(ConcurrentDictionary<string, object>), "module");
                var body = BuildExpression(param, expr, data);
                return Expression.Lambda<Func<ConcurrentDictionary<string, object>, object>>(body, param).Compile();
            });

            return func(data);
        }

        // BuildDataSetter: کامپایل به اکشن
        public Action<object> BuildDataSetter(string path, ConcurrentDictionary<string, object> data)
        {
            return (Action<object>)_compiledCache.GetOrAdd("SET:" + path, expr =>
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
            });
        }

        // ساخت Expression از رشته
        private Expression BuildExpression(ParameterExpression moduleParam, string expr, ConcurrentDictionary<string, object> data)
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
            if (int.TryParse(expr, out var i)) { value = i; return true; }
            if (double.TryParse(expr, out var d)) { value = d; return true; }
            if (bool.TryParse(expr, out var b)) { value = b; return true; }
            if (expr.StartsWith("\"") && expr.EndsWith("\"")) { value = expr.Trim('"'); return true; }

            value = null;
            return false;
        }
    }
}
