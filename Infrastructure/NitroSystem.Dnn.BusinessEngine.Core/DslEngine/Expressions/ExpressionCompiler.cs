using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class ExpressionCompiler : ExpressionCompilerBase
    {
        private static readonly MethodInfo ObjectEqualsMethod =
            typeof(object).GetMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) });

        public override Expression BuildExpression(ExpressionBase expression, ParameterExpression ctx)
        {
            if (expression is LiteralExpression)
            {
                var lit = (LiteralExpression)expression;
                return Expression.Constant(lit.Value);
            }

            if (expression is MemberAccessExpression)
            {
                return BuildMemberAccess(expression, ctx);
            }

            if (expression is FunctionCallExpression)
            {
                return BuildFunctionCall((FunctionCallExpression)expression, ctx);
            }

            if (expression is BinaryExpression)
            {
                return BuildBinary((BinaryExpression)expression, ctx);
            }

            // NEW
            if (expression is UnaryExpression)
            {
                return BuildUnary((UnaryExpression)expression, ctx);
            }

            throw new NotSupportedException("Expression not supported");
        }

        // NEW
        private Expression BuildUnary(UnaryExpression expr, ParameterExpression ctx)
        {
            var operand = BuildExpression(expr.Operand, ctx);

            switch (expr.Operator)
            {
                case "!":
                    // Operand often arrives boxed as "object" (e.g. resolved through
                    // a dictionary-backed member access) — convert before Expression.Not,
                    // same reasoning as the object-vs-value-type handling in BuildBinary.
                    var boolOperand = operand.Type == typeof(bool)
                        ? operand
                        : Expression.Convert(operand, typeof(bool));
                    return Expression.Not(boolOperand);
            }

            throw new InvalidOperationException("Invalid unary operator");
        }

        private Expression BuildBinary1(BinaryExpression expr, ParameterExpression ctx)
        {
            var left = BuildExpression(expr.Left, ctx);
            var right = BuildExpression(expr.Right, ctx);

            left = UnifyTypes(left, right, out right);

            switch (expr.Operator)
            {
                case "+": return Expression.Add(left, right);
                case "-": return Expression.Subtract(left, right);
                case "*": return Expression.Multiply(left, right);
                case "/": return Expression.Divide(left, right);
                case "==": return Expression.Equal(left, right);
                case "!=": return Expression.NotEqual(left, right);
                case ">": return Expression.GreaterThan(left, right);
                case ">=": return Expression.GreaterThanOrEqual(left, right);
                case "<": return Expression.LessThan(left, right);
                case "<=": return Expression.LessThanOrEqual(left, right);
                case "and": return Expression.AndAlso(left, right);
                case "or": return Expression.OrElse(left, right);
            }

            throw new InvalidOperationException("Invalid operator");
        }

        private Expression BuildBinary(BinaryExpression expr, ParameterExpression ctx)
        {
            var left = BuildExpression(expr.Left, ctx);
            var right = BuildExpression(expr.Right, ctx);

            left = UnifyTypes(left, right, out right);

            // When one side is boxed as "object" (e.g. a value pulled from a dictionary like
            // Field.Settings[...]) and the other side is a reference type (e.g. a string
            // literal), Expression.Equal falls back to reference equality instead of value
            // equality, since UnifyTypes only unifies value-type mismatches. Route this
            // specific case through object.Equals, which dispatches to the runtime type's
            // Equals override (e.g. string.Equals) and handles nulls safely.
            bool isObjectVsReference =
                (left.Type == typeof(object) && !right.Type.IsValueType) ||
                (right.Type == typeof(object) && !left.Type.IsValueType);

            if (isObjectVsReference && (expr.Operator == "==" || expr.Operator == "!="))
            {
                var equalsCall = Expression.Call(
                    ObjectEqualsMethod,
                    Expression.Convert(left, typeof(object)),
                    Expression.Convert(right, typeof(object)));

                return expr.Operator == "==" ? (Expression)equalsCall : Expression.Not(equalsCall);
            }

            switch (expr.Operator)
            {
                case "+": return Expression.Add(left, right);
                case "-": return Expression.Subtract(left, right);
                case "*": return Expression.Multiply(left, right);
                case "/": return Expression.Divide(left, right);
                case "==": return Expression.Equal(left, right);
                case "!=": return Expression.NotEqual(left, right);
                case ">": return Expression.GreaterThan(left, right);
                case ">=": return Expression.GreaterThanOrEqual(left, right);
                case "<": return Expression.LessThan(left, right);
                case "<=": return Expression.LessThanOrEqual(left, right);
                case "and": return Expression.AndAlso(left, right);
                case "or": return Expression.OrElse(left, right);
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
                typeof(IExpressionContext).GetMethod(nameof(IExpressionContext.InvokeFunction)),
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

            if (left.Type == typeof(object) && right.Type.IsValueType)
            {
                var nullableType = IsNullable(right.Type)
                    ? right.Type
                    : typeof(Nullable<>).MakeGenericType(right.Type);

                left = Expression.Convert(left, nullableType);
                unifiedRight = Expression.Convert(right, nullableType);
                return left;
            }

            // object vs value type
            if (right.Type == typeof(object) && left.Type.IsValueType)
            {
                var nullableType = IsNullable(left.Type)
                    ? left.Type
                    : typeof(Nullable<>).MakeGenericType(left.Type);

                unifiedRight = Expression.Convert(right, nullableType);
                return Expression.Convert(left, nullableType);
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

            return left;
        }

        private static bool IsNullConstant(Expression expr)
        {
            return expr is ConstantExpression ce && ce.Value == null;
        }

        private static bool IsNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static Type GetWiderType(Type typeA, Type typeB)
        {
            typeA = Nullable.GetUnderlyingType(typeA) ?? typeA;
            typeB = Nullable.GetUnderlyingType(typeB) ?? typeB;

            if (typeA == typeof(double) || typeB == typeof(double)) return typeof(double);
            if (typeA == typeof(float) || typeB == typeof(float)) return typeof(float);
            if (typeA == typeof(long) || typeB == typeof(long)) return typeof(long);
            return typeof(int);
        }
    }
}
