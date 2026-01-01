using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
   public interface IFieldTypePaneGeneration
    {
        Task<string> GeneratePanes(ModuleFieldDto field);
    }
}
