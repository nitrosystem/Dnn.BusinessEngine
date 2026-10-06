using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base
{
    public abstract class ExpressionCompilerBase
    {
        private IExpressionContext _context;
        private readonly Dictionary<ExpressionBase, Func<IExpressionContext, object>> _cache = new();
        protected class Exp : ExpressionBase { }

        public virtual Func<IExpressionContext, object> Compile(ExpressionBase expression, IExpressionContext context)
        {
            _context = context;

            Func<IExpressionContext, object> compiled;
            if (_cache.TryGetValue(expression, out compiled))
                return compiled;

            var ctxParam = Expression.Parameter(typeof(IExpressionContext), "ctx");
            var body = BuildExpression(expression, ctxParam);

            var lambda = Expression.Lambda<Func<IExpressionContext, object>>(
                Expression.Convert(body, typeof(object)), ctxParam);

            compiled = lambda.Compile();
            _cache[expression] = compiled;

            return compiled;
        }

        public virtual Expression BuildExpression(string expression, ParameterExpression ctx)
        {
            var expr = new Exp();
            expr.ExpressionPath = expression;
            return BuildExpression(expr, ctx);
        }

        public virtual Expression BuildExpression(ExpressionBase expr, ParameterExpression ctx)
        {
            if (expr is null)
                throw new ArgumentNullException(nameof(expr), "Expression cannot be null.");

            if (string.IsNullOrEmpty(expr.ExpressionPath))
                throw new ArgumentException("ExpressionPath cannot be null or empty.", nameof(expr));

            string expression = expr.ExpressionPath.Trim();

            // literal
            if (TryParseLiteral(expression, out var lit))
                return Expression.Constant(lit, typeof(object));

            // function
            if (IsFunction(expression, out string funcName, out string[] args))
            {
                if (!ExpressionFunctions.BuiltIn.TryGetValue(funcName, out var fn))
                    throw new InvalidOperationException($"Unknown function {funcName}");

                var parameters = fn.Method.GetParameters();
                if (parameters.Length != args.Length)
                    throw new InvalidOperationException(
                        $"Function '{funcName}' expects {parameters.Length} argument(s), got {args.Length}.");

                var fnConst = Expression.Constant(fn);
                var argExprs = args
                    .Select((exp, i) => Expression.Convert(BuildExpression(exp, ctx), parameters[i].ParameterType))
                    .ToArray();

                return Expression.Convert(Expression.Invoke(fnConst, argExprs), typeof(object));
            }

            // property path
            return BuildMemberAccess(expr, ctx);
        }

        protected Expression BuildMemberAccess(ExpressionBase expr, ParameterExpression ctx)
        {
            string[] parts = expr.ExpressionPath.Split('.');

            // ctx.GetRoot("ArtistCategory")
            var rootCall =
                Expression.Call(
                    ctx,
                    typeof(IExpressionContext).GetMethod(nameof(IExpressionContext.GetRoot)),
                    Expression.Constant(parts[0])
                );

            // Real type(root)
            Type currentType = _context.GetRootType(parts[0]);

            // object -> real type
            Expression current = Expression.Convert(rootCall, currentType);

            // ArtistCategory.IsDisabled.Title ...
            for (int i = 1; i < parts.Length; i++)
            {
                var propName = parts[i];

                // If is Dictionary : TryGetValue with indexer
                if (TypeHelper.TryGetDictionaryTypes(currentType, out var keyT, out var valT))
                {
                    // If key is not string then convert to key type
                    var keyExpr = keyT == typeof(string)
                        ? (Expression)Expression.Constant(propName)
                        : Expression.Convert(Expression.Constant(propName), keyT);

                    // Find TryGetValue Method
                    var tryGetValue = currentType.GetMethod("TryGetValue", new[] { keyT, valT.MakeByRefType() });
                    if (tryGetValue != null)
                    {
                        var dictVar = Expression.Variable(currentType, "dict");
                        var valueVar = Expression.Variable(valT, "val");

                        var assignDict = Expression.Assign(dictVar, current);
                        var callTryGet = Expression.Call(dictVar, tryGetValue, keyExpr, valueVar);

                        // valueVar: if key exists, otherwise default(valT)
                        var pickValue = Expression.Condition(callTryGet, valueVar, Expression.Default(valT));

                        current = Expression.Block(new[] { dictVar, valueVar }, assignDict, pickValue);
                        currentType = valT;
                    }
                    else
                    {
                        // fallback: "Item" Indexer
                        var indexer = currentType.GetProperty("Item", new[] { keyT });
                        if (indexer == null)
                            throw new InvalidOperationException($"No indexer found on dictionary-like type '{currentType.Name}'");

                        current = Expression.Property(current, indexer, keyExpr);
                        currentType = indexer.PropertyType;
                    }

                    continue;
                }

                // If is POCO : Get Property 
                var propInfo = currentType.GetProperty(propName);
                if (propInfo == null)
                    throw new InvalidOperationException($"Property '{propName}' not found on type '{currentType.Name}'");

                current = Expression.Property(current, propInfo);
                currentType = propInfo.PropertyType;
            }

            return current;
        }

        private bool TryParseLiteral(string expr, out object value)
        {
            if (expr.Equals("null", StringComparison.OrdinalIgnoreCase)) { value = null; return true; }
            if (int.TryParse(expr, out var i)) { value = i; return true; }
            if (double.TryParse(expr, out var d)) { value = d; return true; }
            if (bool.TryParse(expr, out var b)) { value = b; return true; }
            if (expr.StartsWith("\"") && expr.EndsWith("\"")) { value = expr.Trim('"'); return true; }

            value = null;
            return false;
        }

        private bool IsFunction(string expr, out string funcName, out string[] args)
        {
            funcName = null;
            args = null;

            var idx = expr.IndexOf('(');
            if (idx > 0 && expr.EndsWith(")"))
            {
                funcName = expr.Substring(0, idx);
                var inner = expr.Substring(idx + 1, expr.Length - idx - 2);

                // Split params => Course.Name, 2, 5
                args = SplitArgs(inner).ToArray();
                return true;
            }

            return false;
        }

        private IEnumerable<string> SplitArgs(string input)
        {
            //separate with commas
            return input.Split(',').Select(x => x.Trim());
        }
    }
}
