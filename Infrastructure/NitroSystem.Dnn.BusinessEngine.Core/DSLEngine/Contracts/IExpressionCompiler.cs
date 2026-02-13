using System;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Contracts
{
    public interface IExpressionCompiler
    {
        Func<IDslContext, object> Compile(DslExpression expression);
    }
}
