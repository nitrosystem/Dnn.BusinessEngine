using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Expressions
{
    public sealed class LiteralExpression : DslExpression
    {
        public object Value { get; set; }
    }
}
