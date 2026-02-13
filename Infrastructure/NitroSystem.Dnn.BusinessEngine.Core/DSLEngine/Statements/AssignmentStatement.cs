using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements
{
    public sealed class AssignmentStatement : DslStatement
    {
        public MemberAccessExpression Target { get; set; }
        public DslExpression Value { get; set; }
    }
}
