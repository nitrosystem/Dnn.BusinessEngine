using System.Text;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Shared.Helpers;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Functions;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes
{
    public class FunctionNode : Node
    {
        public string FunctionName { get; }
        public string Args { get; }

        public FunctionNode(string functionName, string args)
        {
            FunctionName = functionName;
            Args = args;
        }

        public override void Render(StringBuilder sb, TemplateContext context)
        {
            var args = new List<object>();
            foreach (var item in Args?.Split(','))
            {
                var parsedArgs = ExpressionInterpolator.Interpolate(item, context);
                if (!string.IsNullOrEmpty(parsedArgs))
                {
                    var correctValue = TypeHelper.ConvertValue(parsedArgs); 
                    args.Add(correctValue);
                }
            }
            var fnResult = ExpressionFunctions.BuiltIn[FunctionName].DynamicInvoke(args.ToArray());
            sb.Append(fnResult);
        }
    }
}