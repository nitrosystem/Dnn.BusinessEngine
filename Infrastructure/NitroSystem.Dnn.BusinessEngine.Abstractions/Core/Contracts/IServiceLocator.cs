namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts
{
    public interface IServiceLocator
    {
        T GetInstance<T>(string typeName) where T : class;
        T CreateInstance<T>(string typeName, params object[] parameters) where T : class;
    }
}
