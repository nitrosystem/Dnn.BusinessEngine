using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class FunctionCallExpression : DslExpression
    {
        public string FunctionName { get; set; }
        public List<DslExpression> Arguments { get; set; }
    }
}
