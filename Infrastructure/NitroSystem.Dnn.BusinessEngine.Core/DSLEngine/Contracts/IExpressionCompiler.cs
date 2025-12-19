using System;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Contracts
{
    public interface IExpressionCompiler
    {
        Func<IDslContext, object> Compile(DslExpression expression);
    }
}
