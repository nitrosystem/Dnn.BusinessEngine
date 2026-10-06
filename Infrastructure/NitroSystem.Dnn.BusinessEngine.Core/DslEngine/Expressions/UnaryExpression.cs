using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class UnaryExpression : ExpressionBase
    {
        public string Operator { get; set; }
        public ExpressionBase Operand { get; set; }
    }
}
