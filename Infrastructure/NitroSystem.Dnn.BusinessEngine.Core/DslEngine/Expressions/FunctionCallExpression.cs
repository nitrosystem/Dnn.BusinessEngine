using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class FunctionCallExpression : ExpressionBase
    {
        public string FunctionName { get; set; }
        public List<ExpressionBase> Arguments { get; set; }
    }
}
