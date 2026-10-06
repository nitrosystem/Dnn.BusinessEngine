using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements
{
    public sealed class FunctionCallStatement : DslStatement
    {
        public string FunctionName { get; set; }
        public IReadOnlyList<ExpressionBase> Arguments { get; set; }
    }
}
