using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class ExpressionCompiler : IExpressionCompiler
    {
        private readonly IDslContext _context;

        private readonly Dictionary<DslExpression, Func<IDslContext, object>> _cache
            = new Dictionary<DslExpression, Func<IDslContext, object>>();

        public ExpressionCompiler(IDslContext context)
        {
            _context = context;
        }

        public Func<IDslContext, object> Compile(DslExpression expression)
        {
            Func<IDslContext, object> compiled;
            if (_cache.TryGetValue(expression, out compiled))
                return compiled;

            var ctxParam = Expression.Parameter(typeof(IDslContext), "ctx");
            var body = BuildExpression(expression, ctxParam);

            var lambda = Expression.Lambda<Func<IDslContext, object>>(
                Expression.Convert(body, typeof(object)), ctxParam);

            compiled = lambda.Compile();
            _cache[expression] = compiled;

            return compiled;
        }

        private Expression BuildExpression(DslExpression expr, ParameterExpression ctx)
        {
            if (expr is LiteralExpression)
            {
                var lit = (LiteralExpression)expr;
                return Expression.Constant(lit.Value);
            }

            if (expr is MemberAccessExpression)
            {
                return BuildMemberAccess((MemberAccessExpression)expr, ctx);
            }

            if (expr is FunctionCallExpression)
            {
                return BuildFunctionCall((FunctionCallExpression)expr, ctx);
            }

            if (expr is BinaryExpression)
            {
                return BuildBinary((BinaryExpression)expr, ctx);
            }

            throw new NotSupportedException("Expression not supported");
        }

        private static bool TryGetDictionaryTypes(Type type, out Type keyType, out Type valueType)
        {
            foreach (var itf in type.GetInterfaces().Concat(new[] { type }))
            {
                if (itf.IsGenericType)
                {
                    var def = itf.GetGenericTypeDefinition();
                    if (def == typeof(IDictionary<,>) || def == typeof(IReadOnlyDictionary<,>))
                    {
                        var args = itf.GetGenericArguments();
                        keyType = args[0]; valueType = args[1];
                        return true;
                    }
                }
            }
            keyType = null; valueType = null;
            return false;
        }

        private Expression BuildMemberAccess(MemberAccessExpression expr, ParameterExpression ctx)
        {
            // ctx.GetRoot("ArtistCategory")
            var rootCall =
                Expression.Call(
                    ctx,
                    typeof(IDslContext).GetMethod(nameof(IDslContext.GetRoot)),
                    Expression.Constant(expr.Path[0])
                );

            // نوع واقعی root
            Type currentType = _context.GetRootType(expr.Path[0]);

            // object -> real type
            Expression current = Expression.Convert(rootCall, currentType);

            // ArtistCategory.IsDisabled.Title ...
            for (int i = 1; i < expr.Path.Count; i++)
            {
                var propName = expr.Path[i];

                // null-propagation: اگر current == null، نتیجه‌ی مرحله‌ی بعدی را default برگردان
                // این را فقط برای انواع reference اعمال می‌کنیم
                if (!currentType.IsValueType || Nullable.GetUnderlyingType(currentType) != null)
                {
                    var tmp = Expression.Variable(currentType, "tmp");
                    var assign = Expression.Assign(tmp, current);
                    var whenNull = Expression.Default(typeof(object)); // خروجی موقت؛ بعداً cast می‌کنیم
                    var blockStart = Expression.Block(new[] { tmp }, assign);
                    // block را بعد از تعیین current مرحله بعد کامل می‌کنیم
                    // برای سادگی، ادامه‌ی مسیر را بدون این لایه نشان می‌دهیم؛ در صورت نیاز می‌توانی این الگو را به‌صورت helper دربیاری.
                }

                // اگر دیکشنری است: با TryGetValue یا ایندکسر بخوان
                if (TryGetDictionaryTypes(currentType, out var keyT, out var valT))
                {
                    // اگر کلید string نیست، به نوع کلید تبدیل کن
                    var keyExpr = keyT == typeof(string)
                        ? (Expression)Expression.Constant(propName)
                        : Expression.Convert(Expression.Constant(propName), keyT);

                    // متد TryGetValue را پیدا کن
                    var tryGetValue = currentType.GetMethod("TryGetValue", new[] { keyT, valT.MakeByRefType() });
                    if (tryGetValue != null)
                    {
                        var dictVar = Expression.Variable(currentType, "dict");
                        var valueVar = Expression.Variable(valT, "val");

                        var assignDict = Expression.Assign(dictVar, current);
                        var callTryGet = Expression.Call(dictVar, tryGetValue, keyExpr, valueVar);

                        // اگر کلید وجود داشت val، در غیر این صورت default(valT)
                        var pickValue = Expression.Condition(callTryGet, valueVar, Expression.Default(valT));

                        current = Expression.Block(new[] { dictVar, valueVar }, assignDict, pickValue);
                        currentType = valT;
                    }
                    else
                    {
                        // fallback: ایندکسر "Item"
                        var indexer = currentType.GetProperty("Item", new[] { keyT });
                        if (indexer == null)
                            throw new InvalidOperationException($"No indexer found on dictionary-like type '{currentType.Name}'");

                        current = Expression.Property(current, indexer, keyExpr);
                        currentType = indexer.PropertyType;
                    }

                    continue;
                }

                // اگر POCO است: property را بخوان
                var propInfo = currentType.GetProperty(propName);
                if (propInfo == null)
                    throw new InvalidOperationException($"Property '{propName}' not found on type '{currentType.Name}'");

                current = Expression.Property(current, propInfo);
                currentType = propInfo.PropertyType;

                // اگر مقدار object برگشت و مسیر ادامه دارد، می‌توانی در مرحله‌ی بعدی cast runtime اضافه کنی
                // مثلاً وقتی مقدار از دیکشنری به صورت object آمده و انتظار POCO داریم:
                // current = Expression.Convert(current, expectedTypeInNextStep);
            }

            return current;
        }

        private Expression BuildMemberAccess2(
            MemberAccessExpression expr,
            ParameterExpression ctx)
        {
            // ctx.GetRoot("ArtistCategory")
            var rootCall =
                Expression.Call(
                    ctx,
                    typeof(IDslContext).GetMethod(nameof(IDslContext.GetRoot)),
                    Expression.Constant(expr.Path[0])
                );

            // type واقعی root
            Type currentType = _context.GetRootType(expr.Path[0]);

            // object -> real type
            Expression current = Expression.Convert(rootCall, currentType);

            // ArtistCategory.IsDisabled.Title ...
            for (int i = 1; i < expr.Path.Count; i++)
            {
                var propName = expr.Path[i];

                var propInfo = currentType.GetProperty(propName);
                if (propInfo == null)
                    throw new InvalidOperationException(
                        $"Property '{propName}' not found on type '{currentType.Name}'");

                current = Expression.Property(current, propInfo);
                currentType = propInfo.PropertyType;
            }

            return current;
        }

        private Expression BuildMemberAccess1(MemberAccessExpression expr, ParameterExpression ctx)
        {
            Expression current =
                Expression.Call(ctx,
                    typeof(IDslContext).GetMethod("GetRoot"),
                    Expression.Constant(expr.Path[0]));

            for (int i = 1; i < expr.Path.Count; i++)
            {
                current = Expression.Property(current, expr.Path[i]);
            }

            return current;
        }

        private Expression BuildBinary(BinaryExpression expr, ParameterExpression ctx)
        {
            var left = BuildExpression(expr.Left, ctx);
            var right = BuildExpression(expr.Right, ctx);

            left = UnifyTypes(left, right, out right);

            switch (expr.Operator)
            {
                case "==": return Expression.Equal(left, right);
                case "!=": return Expression.NotEqual(left, right);
                case ">": return Expression.GreaterThan(left, right);
                case "<": return Expression.LessThan(left, right);
                case "&&": return Expression.AndAlso(left, right);
                case "||": return Expression.OrElse(left, right);
            }

            throw new InvalidOperationException("Invalid operator");
        }

        private Expression BuildFunctionCall(
            FunctionCallExpression expr,
            ParameterExpression ctx)
        {
            var args = expr.Arguments
                .Select(a => BuildExpression(a, ctx))
                .Select(e => Expression.Convert(e, typeof(object)))
                .ToArray();

            var argsArray =
                Expression.NewArrayInit(typeof(object), args);

            return Expression.Call(
                ctx,
                typeof(IDslContext).GetMethod(nameof(IDslContext.InvokeFunction)),
                Expression.Constant(expr.FunctionName),
                argsArray
            );
        }

        private static Expression UnifyTypes(
            Expression left,
            Expression right,
            out Expression unifiedRight)
        {
            unifiedRight = right;

            // object vs value type
            if (left.Type == typeof(object) && right.Type.IsValueType)
            {
                left = Expression.Convert(left, right.Type);
                return left;
            }

            if (right.Type == typeof(object) && left.Type.IsValueType)
            {
                unifiedRight = Expression.Convert(right, left.Type);
                return left;
            }

            if (left.Type == right.Type)
                return left;

            if (IsNullable(left.Type) &&
                Nullable.GetUnderlyingType(left.Type) == right.Type)
            {
                unifiedRight = Expression.Convert(right, left.Type);
                return left;
            }

            if (IsNullable(right.Type) &&
                Nullable.GetUnderlyingType(right.Type) == left.Type)
            {
                return Expression.Convert(left, right.Type);
            }

            if (left.Type.IsValueType && right.Type.IsValueType)
            {
                var targetType = GetWiderType(left.Type, right.Type);
                unifiedRight = Expression.Convert(right, targetType);
                return Expression.Convert(left, targetType);
            }

            throw new InvalidOperationException(
                $"Cannot compare types '{left.Type}' and '{right.Type}'");
        }


        private static Expression UnifyTypesOld(
            Expression left,
            Expression right,
            out Expression unifiedRight)
        {
            unifiedRight = right;

            if (left.Type == right.Type)
                return left;

            // Nullable<T> == T
            if (IsNullable(left.Type) &&
                Nullable.GetUnderlyingType(left.Type) == right.Type)
            {
                unifiedRight = Expression.Convert(right, left.Type);
                return left;
            }

            // T == Nullable<T>
            if (IsNullable(right.Type) &&
                Nullable.GetUnderlyingType(right.Type) == left.Type)
            {
                return Expression.Convert(left, right.Type);
            }

            // both value types (int, long, short ...)
            if (left.Type.IsValueType && right.Type.IsValueType)
            {
                var targetType = GetWiderType(left.Type, right.Type);

                return Expression.Convert(left, targetType);
            }

            throw new InvalidOperationException(
                $"Cannot compare types '{left.Type}' and '{right.Type}'");
        }

        private static bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static Type GetWiderType(Type a, Type b)
        {
            a = Nullable.GetUnderlyingType(a) ?? a;
            b = Nullable.GetUnderlyingType(b) ?? b;

            if (a == typeof(double) || b == typeof(double)) return typeof(double);
            if (a == typeof(float) || b == typeof(float)) return typeof(float);
            if (a == typeof(long) || b == typeof(long)) return typeof(long);
            return typeof(int);
        }
    }
}
