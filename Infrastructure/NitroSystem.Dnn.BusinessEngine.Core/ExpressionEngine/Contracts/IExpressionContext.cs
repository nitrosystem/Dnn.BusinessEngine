using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Contracts
{
    public interface IExpressionContext
    {
        object GetRoot(string name);
        Type GetRootType(string name);
        void SetRoot(string name, object value);
        object InvokeFunction(string name, object[] args);
    }
}
