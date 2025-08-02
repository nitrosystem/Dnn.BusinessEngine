using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionWorker
    {
        Task CallActions(IModuleData moduleData, Guid moduleId, Guid? fieldId, string eventName);

        Task CallActions(IEnumerable<Guid> actionIds, IModuleData moduleData);

        Task CallAction(IModuleData moduleData, Queue<ActionTree> buffer);
    }
}
