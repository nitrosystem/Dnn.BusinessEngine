using System.Linq.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ConditionParser
{
    public class LogicalNode : ConditionNode
    {
        public ConditionNode Left { get; set; }
        public string Operator { get; set; }
        public ConditionNode Right { get; set; }

        public override Expression BuildExpression(ParameterExpression param)
        {
            var leftExpr = Left.BuildExpression(param);
            var rightExpr = Right.BuildExpression(param);

            switch (Operator)
            {
                case "&&":
                    return Expression.AndAlso(leftExpr, rightExpr);
                case "||":
                    return Expression.OrElse(leftExpr, rightExpr);
                default:
                    return null;
            }
        }
    }
}
