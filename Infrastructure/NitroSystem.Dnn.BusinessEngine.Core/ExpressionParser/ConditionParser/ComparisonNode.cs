using System;
using System.Linq.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ConditionParser
{
    public class ComparisonNode : ConditionNode
    {
        public string FieldName { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }

        public override Expression BuildExpression(ParameterExpression param)
        {
            var member = GetNestedPropertyExpression(param, FieldName);

            if (Value?.ToString() == "null") Value = null;

            Expression constant = Expression.Constant(Value != null ? Convert.ChangeType(Value, member.Type) : null, member.Type);

            if (Value == null)
            {
                switch (Operator)
                {
                    case "==":
                        return Expression.Equal(member, Expression.Constant(null, typeof(object))); // ReferenceEqual هم میشه
                    case "!=":
                        return Expression.NotEqual(member, Expression.Constant(null, typeof(object))); // ReferenceNotEqual هم میشه
                }
            }

            switch (Operator)
            {
                case "==":
                    return Expression.Equal(member, constant);
                case "!=":
                    return Expression.NotEqual(member, constant);
                case ">":
                    return Expression.GreaterThan(member, constant);
                case "<":
                    return Expression.LessThan(member, constant);
                case ">=":
                    return Expression.GreaterThanOrEqual(member, constant);
                case "<=":
                    return Expression.LessThanOrEqual(member, constant);
            }

            return null;
        }


        private Expression GetNestedPropertyExpression(Expression param, string propertyPath)
        {
            foreach (var property in propertyPath.Split('.'))
            {
                param = Expression.Property(param, property);
            }
            return param;
        }
    }
}
