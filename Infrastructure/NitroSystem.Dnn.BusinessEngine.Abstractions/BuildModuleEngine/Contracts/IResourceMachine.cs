using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Contracts
{
    public interface IResourceMachine
    {
        Task<List<ResourceDto>> RunAsync(IEnumerable<ResourceDto> resources, HttpContext context);
    }
}
