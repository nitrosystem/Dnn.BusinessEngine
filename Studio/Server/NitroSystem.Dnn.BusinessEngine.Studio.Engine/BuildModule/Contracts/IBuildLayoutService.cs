using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildLayoutService
    {
        Task<string> BuildLayoutAsync(ModuleDto module, int userId, Action<string, double> progress);
    }
}
