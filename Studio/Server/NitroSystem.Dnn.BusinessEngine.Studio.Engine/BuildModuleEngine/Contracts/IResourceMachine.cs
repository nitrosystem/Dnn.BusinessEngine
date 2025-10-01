using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Contracts
{
    public interface IResourceMachine
    {
        Task<List<ModuleResourceInfo>> RunAsync(IEnumerable<MachineResourceInfo> resources, HttpContext context);
    }
}
