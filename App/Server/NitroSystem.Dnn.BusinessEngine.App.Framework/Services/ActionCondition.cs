using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Common.Globals;

namespace NitroSystem.Dnn.BusinessEngine.App.Framework.Services
{
    public class ActionCondition : IActionCondition
    {
        private readonly IExpressionService _expressionService;

        public ActionCondition(IExpressionService expressionService)
        {
            this._expressionService = expressionService;
        }

        public bool IsTrueConditions(IModuleData moduleData, IEnumerable<ExpressionInfo> conditions)
        {
            if (conditions == null || !conditions.Any()) return true;

            bool andResult = true;

            var groups = conditions.GroupBy(c => c.GroupName);
            foreach (var group in groups)
            {
                bool orResult = false;

                foreach (var condition in group)
                {
                    var leftValue = this._expressionService.Evaluate(moduleData,condition.LeftExpression);

                    var rightValue = this._expressionService.Evaluate(moduleData, condition.RightExpression);

                    bool compareResult = SmartValueComparer.Compare(leftValue, rightValue, condition.EvalType);

                    if (!orResult && compareResult) orResult = true;
                }

                if (!orResult)
                {
                    andResult = false;
                    break;
                }
            }

            return andResult;
        }
    }
}
