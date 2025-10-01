using System.Linq.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ExpressionParser.ConditionParser
{
    public abstract class ConditionNode  
    {
        public abstract Expression BuildExpression(ParameterExpression param);
    }
}
