using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Contracts
{
    public interface IMergeResourcesService
    {
        Task<(string Scripts, string Styles)> MergeResourcesAsync(ModuleDto module, int userId, IEnumerable<ModuleResourceDto> resources, Action<string, double> progress = null);
    }
}
