using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionCondition
    {
        bool IsTrueConditions(IEnumerable<ExpressionInfo> conditions);

        bool CompareTwoValue(string leftValue, string rightValue, string operatorType);
    }
}
