using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements
{
    public sealed class IfStatement : DslStatement
    {
        public ExpressionBase Condition { get; set; }
        public IReadOnlyList<DslStatement> Then { get; set; }
        public IReadOnlyList<DslStatement> Else { get; set; }
    }

}
