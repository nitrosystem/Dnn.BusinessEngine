using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements
{
    public sealed class IfStatement : DslStatement
    {
        public DslExpression Condition { get; set; }
        public IReadOnlyList<DslStatement> Then { get; set; }
        public IReadOnlyList<DslStatement> Else { get; set; }
    }

}
