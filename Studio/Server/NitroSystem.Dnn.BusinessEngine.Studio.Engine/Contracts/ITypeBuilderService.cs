using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface ITypeBuilderService
    {
        string BuildAppModelAsType(AppModelDto appModel, string ouputPath);
    }
}
