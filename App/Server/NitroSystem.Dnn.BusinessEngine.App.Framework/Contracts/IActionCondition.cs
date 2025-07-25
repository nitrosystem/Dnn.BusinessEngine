using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionCondition
    {
        bool IsTrueConditions(IEnumerable<IExpression> conditions);

        bool CompareTwoValue(string leftValue, string rightValue, string operatorType);
    }
}
