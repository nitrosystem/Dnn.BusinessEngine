using NitroSystem.Dnn.BusinessEngine.App.Services.Dto.Action;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public interface IActionWorker
    {
        Task<object> CallActions(string connectionId, Guid moduleId, Guid? fieldId, string eventName, bool isServerSide);

        //Task<object> CallAction(Guid actionID);
        
        Task<object> CallAction(string connectionId, Queue<ActionTree> buffer);

        IAction CreateInstance(string actionType);

        //void SetActionResults(ActionDto action, dynamic data);
    }
}
