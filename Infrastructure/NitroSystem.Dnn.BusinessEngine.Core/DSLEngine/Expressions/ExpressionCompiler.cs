using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Expressions
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

            if (expr is BinaryExpression)
            {
                return BuildBinary((BinaryExpression)expr, ctx);
            }

            throw new NotSupportedException("Expression not supported");
        }

        private Expression BuildMemberAccess(
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
                case "&&": return Expression.AndAlso(left, right);
                case "||": return Expression.OrElse(left, right);
            }

            throw new InvalidOperationException("Invalid operator");
        }

        private static Expression UnifyTypes(
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
