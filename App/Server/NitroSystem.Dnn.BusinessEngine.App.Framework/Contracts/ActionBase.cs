using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.App.Services.Dto.Action;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public abstract class ActionBase<T> : IAction
    {
        protected ActionDto Action { get; set; }
        protected virtual T Model { get; set; }

        public event EventHandler<ActionEventArgs> OnActionSuccessEvent;
        public event EventHandler<ActionEventArgs> OnActionErrorEvent;
        public event EventHandler<ActionEventArgs> OnActionCompletedEvent;

        public void Init(ActionDto action)
        {
            this.Action = action;

            try
            {
                if (!string.IsNullOrEmpty(action.Settings)) this.Model = JsonConvert.DeserializeObject<T>(action.Settings);
            }
            catch (Exception ex)
            {
                this.TryParseModel(action.Settings);
            }
        }

        public abstract bool TryParseModel(string actionDetails);

        public abstract Task<object> ExecuteAsync<T>();

        public void OnActionSuccess()
        {
            OnActionSuccessEvent?.Invoke(this, new ActionEventArgs());
        }

        public void OnActionError()
        {
            OnActionErrorEvent?.Invoke(this, new ActionEventArgs());
        }

        public void OnActionCompleted()
        {
            OnActionCompletedEvent?.Invoke(this, new ActionEventArgs());
        }
    }
}
