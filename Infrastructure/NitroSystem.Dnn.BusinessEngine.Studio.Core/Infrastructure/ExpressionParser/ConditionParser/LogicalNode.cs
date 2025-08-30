using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser
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
            };

            return null;
        }
    }
}
