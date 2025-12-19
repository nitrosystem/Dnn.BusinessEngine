using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Expressions
{
    public sealed class BinaryExpression : DslExpression
    {
        public DslExpression Left { get; set; }
        public string Operator { get; set; }
        public DslExpression Right { get; set; }
    }
}
