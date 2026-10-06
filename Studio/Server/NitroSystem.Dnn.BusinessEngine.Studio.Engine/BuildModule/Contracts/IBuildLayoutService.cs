using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildLayoutService
    {
        Task<string> BuildLayoutAsync(IEngineContext context, ModuleDto module, Action<string, double> progress);
    }
}
