using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Contracts
{
    public interface IMergeResourcesService
    {
        Task<(string Scripts, string Styles)> MergeResourcesAsync(Guid moduleId, int userId, IEnumerable<ModuleResourceDto> resources);
    }
}
