using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models
{
    public class ActionException
    {
        public string ErrorMessage { get; set; }
        public string StateString { get { return State.ToString(); } }
        public ActionExceptionState State { get; set; }
        public IEnumerable<ActionParamDto> Params { get; }
        public Exception Exception { get; set; }

        public ActionException(string message, ActionExceptionState state, Exception Exception = null)
        {
            ErrorMessage = message;
            State = state;
        }

        public ActionException(string message, ActionExceptionState state, IEnumerable<ActionParamDto> actionParams = null, Exception Exception = null)
        {
            ErrorMessage = message;
            State = state;
            Params = actionParams;
        }
    }
}
