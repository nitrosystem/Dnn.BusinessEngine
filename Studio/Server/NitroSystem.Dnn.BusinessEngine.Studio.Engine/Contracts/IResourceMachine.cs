using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Contracts
{
    public interface IResourceMachine
    {
        Task<List<ModuleResourceInfo>> RunAsync(IEnumerable<MachineResourceInfo> resources, HttpContext context);
    }
}
