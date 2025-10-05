using System.Linq;
using System.Collections.Concurrent;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.App.Framework.Services
{
    public class ActionCondition : IActionCondition
    {
        private readonly IExpressionService _expressionService;

        public ActionCondition(IExpressionService expressionService)
        {
            this._expressionService = expressionService;
        }

        public bool IsTrueConditions(ConcurrentDictionary<string, object> moduleData, string conditions)
        {
            return true;

            if (conditions == null || !conditions.Any()) return true;

            //bool andResult = true;

            //var groups = conditions.GroupBy(c => c.GroupName);
            //foreach (var group in groups)
            //{
            //    bool orResult = false;

            //    foreach (var condition in group)
            //    {
            //        var leftValue = this._expressionService.Evaluate(moduleData,condition.LeftExpression);

            //        var rightValue = this._expressionService.Evaluate(moduleData, condition.RightExpression);

            //        bool compareResult = SmartValueComparer.Compare(leftValue, rightValue, condition.EvalType);

            //        if (!orResult && compareResult) orResult = true;
            //    }

            //    if (!orResult)
            //    {
            //        andResult = false;
            //        break;
            //    }
            //}

            //return andResult;
        }
    }
}
