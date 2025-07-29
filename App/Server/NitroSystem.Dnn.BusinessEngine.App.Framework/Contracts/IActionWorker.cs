using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionWorker
    {
        Task<object> CallActions(IModuleData moduleData, Guid moduleId, Guid? fieldId, string eventName, bool isServerSide);

        //Task<object> CallAction(Guid actionID);
        
        Task<object> CallAction(IModuleData moduleData, Queue<ActionTree> buffer);

        //void SetActionResults(ActionDto action, dynamic data);
    }
}
