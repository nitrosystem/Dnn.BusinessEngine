using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Database
{
    public class SubmitEntityAction : ActionBase<DatabaseInfo>, IAction
    {
        public SubmitEntityAction()
        {
            this.OnActionCompletedEvent += SubmitEntityAction_OnActionCompletedEvent;
        }

        public SubmitEntityAction(IServiceWorker serviceWorker, IActionWorker actionWorker, ActionDto action)
        {
            this.ServiceWorker = serviceWorker;
            this.ActionWorker = actionWorker;
            this.Action = action;

            this.OnActionCompletedEvent += SubmitEntityAction_OnActionCompletedEvent;
        }

        public override async Task<object> ExecuteAsync<T>(bool isServerSide)
        {
            var actionResult = await this.ServiceWorker.RunServiceByAction<T>(this.Action);

            this.ActionWorker.SetActionResults(this.Action, actionResult);

            return actionResult;
        }

        public override bool TryParseModel(string actionDetails)
        {
            throw new NotImplementedException();
        }

        private void SubmitEntityAction_OnActionCompletedEvent(object sender, ActionEventArgs e)
        {
        }
    }
}
