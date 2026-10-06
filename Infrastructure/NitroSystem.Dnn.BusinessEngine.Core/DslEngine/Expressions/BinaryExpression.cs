using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class BinaryExpression : ExpressionBase
    {
        public ExpressionBase Left { get; set; }
        public string Operator { get; set; }
        public ExpressionBase Right { get; set; }
    }
}
