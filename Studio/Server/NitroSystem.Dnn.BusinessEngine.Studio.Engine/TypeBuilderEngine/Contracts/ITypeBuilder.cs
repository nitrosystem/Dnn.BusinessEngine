using NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.TypeBuilderEngine.Contracts
{
    public interface ITypeBuilder
    {
        string BuildAppModelAsType(AppModelDto appModel, string ouputPath);
    }
}
