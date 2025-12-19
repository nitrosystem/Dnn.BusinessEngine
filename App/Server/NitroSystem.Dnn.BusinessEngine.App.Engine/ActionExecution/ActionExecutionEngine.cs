using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Models;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionExecutionEngine : EngineBase<ActionRequest, ActionResponse>
    {
        protected override void ConfigurePipeline(EnginePipeline<ActionRequest, ActionResponse> pipeline)
        {
            pipeline
                .Use<ActionConditionMiddleware>()
                .Use<ActionSetParamsMiddleware>()
                .Use<ActionWorkerMiddleware>()
                .Use<ActionSetResultMiddleware>();
        }

        public override ActionResponse CreateEmptyResponse()
        {
            return new ActionResponse();
        }

        protected override Task OnErrorAsync(
            Exception ex,
            IEngineContext context,
            ActionResponse response)
        {
            response.Status = ActionResultStatus.Error;
            response.CompletionPhase = context.CurrentPhase.ToString();
            response.CompletionMiddleware = context.CurrentMiddleware;
            response.ErrorException =
                new ActionException(
                    ex.Message,
                    ActionExceptionState.Execution,
                    context.Get<List<ActionParamDto>>("ParsedParams"),
                    ex
                );

            return Task.CompletedTask;
        }
    }
}
