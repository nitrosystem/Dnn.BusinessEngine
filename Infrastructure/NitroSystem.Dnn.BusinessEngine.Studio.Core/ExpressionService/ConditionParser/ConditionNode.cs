using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser
{
    public abstract class ConditionNode  
    {
        public abstract Expression BuildExpression(ParameterExpression param);
    }
}
