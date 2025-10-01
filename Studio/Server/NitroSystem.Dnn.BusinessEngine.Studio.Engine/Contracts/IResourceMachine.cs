using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IResourceMachine
    {
        Task<List<ModuleResourceInfo>> RunAsync(IEnumerable<MachineResourceInfo> resources, HttpContext context);
    }
}
