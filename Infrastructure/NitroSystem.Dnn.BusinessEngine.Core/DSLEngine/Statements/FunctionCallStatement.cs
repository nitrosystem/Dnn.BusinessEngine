using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements
{
    public sealed class FunctionCallStatement : DslStatement
    {
        public string FunctionName { get; set; }
        public IReadOnlyList<DslExpression> Arguments { get; set; }
    }
}
