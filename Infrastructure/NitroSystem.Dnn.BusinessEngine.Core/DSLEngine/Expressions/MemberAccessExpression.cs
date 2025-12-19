using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Expressions
{
    public sealed class MemberAccessExpression : DslExpression
    {
        public IReadOnlyList<string> Path { get; set; }
    }
}
