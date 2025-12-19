using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Statements
{
    public sealed class AssignmentStatement : DslStatement
    {
        public MemberAccessExpression Target { get; set; }
        public DslExpression Value { get; set; }
    }
}
