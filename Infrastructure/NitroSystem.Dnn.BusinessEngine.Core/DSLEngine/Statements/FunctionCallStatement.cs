using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Statements
{
    public sealed class FunctionCallStatement : DslStatement
    {
        public string FunctionName { get; set; }
        public IReadOnlyList<DslExpression> Arguments { get; set; }
    }
}
