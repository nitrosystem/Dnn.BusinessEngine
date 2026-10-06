using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions
{
    public sealed class MemberAccessExpression : ExpressionBase
    {
        public IReadOnlyList<string> Path { get; set; }
        public override string ExpressionPath
        {
            get
            {
                return string.Join(".", Path);
            }
        }
    }
}
