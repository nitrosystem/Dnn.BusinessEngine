using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IBuildLayoutService
    {
        Task<string> BuildLayoutAsync(ModuleDto module, int userId, IEngineNotifier engineNotifier);
    }
}
