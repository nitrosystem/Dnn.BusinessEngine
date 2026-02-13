using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class MemberAccessExpression : DslExpression
    {
        public IReadOnlyList<string> Path { get; set; }
    }
}
