using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class LiteralExpression : ExpressionBase
    {
        public object Value { get; set; }
    }
}
