using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Models;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Repository;
using NitroSystem.Dnn.BusinessEngine.Studio.ApplicationCore.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repositories;
using NitroSystem.Dnn.BusinessEngine.Framework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using NitroSystem.Dnn.BusinessEngine.Framework.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Actions.Database
{
    public class CustomQueryAction : ActionBase<RunServiceInfo>, IAction
    {
        public CustomQueryAction()
        { 
            this.OnActionCompletedEvent += RunService_OnActionCompletedEvent;
        }

        public CustomQueryAction(IServiceWorker serviceWorker, IActionWorker actionWorker, ActionDto action)
        {
            this.ServiceWorker = serviceWorker;
            this.ActionWorker = actionWorker;
            this.Action = action;

            this.OnActionCompletedEvent += RunService_OnActionCompletedEvent;
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

        private void RunService_OnActionCompletedEvent(object sender, ActionEventArgs e)
        {
        }
    }
}
