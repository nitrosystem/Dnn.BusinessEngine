using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class LiteralExpression : DslExpression
    {
        public object Value { get; set; }
    }
}
