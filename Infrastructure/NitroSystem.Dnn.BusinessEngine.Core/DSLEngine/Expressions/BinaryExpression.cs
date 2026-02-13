using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class BinaryExpression : DslExpression
    {
        public DslExpression Left { get; set; }
        public string Operator { get; set; }
        public DslExpression Right { get; set; }
    }
}
