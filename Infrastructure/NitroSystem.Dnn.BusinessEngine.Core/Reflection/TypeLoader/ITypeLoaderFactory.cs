using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeLoader
{
    public interface ITypeLoaderFactory
    {
        Type GetTypeFromAssembly(string relativePath, string typeFullName);
        void ClearAll();
    }
}
