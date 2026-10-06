using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements
{
    public sealed class AssignmentStatement : DslStatement
    {
        public MemberAccessExpression Target { get; set; }
        public ExpressionBase Value { get; set; }
    }
}
