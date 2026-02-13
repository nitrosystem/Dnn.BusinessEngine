using System.Text;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionParser.ExpressionBuilder;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Linq.Expressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
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

        public override void Render(StringBuilder sb, RenderContext context)
        {
            var parsedArgs = ExpressionInterpolator.Interpolate(Args, context);
            var fnResult = ExpressionFunctions.BuiltIn[FunctionName].DynamicInvoke(parsedArgs?.Split(','));

            sb.Append(fnResult);
        }
    }
}