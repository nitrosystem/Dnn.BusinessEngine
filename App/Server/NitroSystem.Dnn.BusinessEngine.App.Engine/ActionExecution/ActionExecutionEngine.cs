using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;
using NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution.Middlewares;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.App.Engine.ActionExecution
{
    public class ActionExecutionEngine : EngineBase<ActionRequest, ActionResponse>
    {
        private readonly bool _raiseException;

        public ActionExecutionEngine(IDiagnosticStore diagnosticStore, bool raiseException)
            : base(diagnosticStore)
        {
            _raiseException = raiseException;
        }

        protected override void ConfigurePipeline(EnginePipeline<ActionRequest, ActionResponse> pipeline)
        {
            pipeline
                .Use<ActionConditionMiddleware>()
                .Use<BeforeExecuteActionMiddleware>()
                .Use<ActionSetParamsMiddleware>()
                .Use<ActionWorkerMiddleware>()
                .Use<ActionSetResultsMiddleware>();
        }

        protected override ActionResponse CreateEmptyResponse()
        {
            return new ActionResponse();
        }

        protected async override Task OnErrorAsync(
            IEngineContext context,
            ActionRequest request,
            ActionResponse response,
            Exception ex
            )
        {
            var entry =
                   DiagnosticEntryBuilder
                       .Runtime()
                       .Error("BE-ACT-001", "Action execution failed")
                       .From(
                           module: "ActionExecutionEngine",
                           component: context.CurrentMiddleware,
                           operation: "Execute")
                       .WithTraceId(TraceId)
                       .WithContext(ctx =>
                       {
                           ctx.ModuleId = request.Action.ModuleId;
                           ctx.EntryId = request.Action.Id;
                           ctx.UserId = request.UserId;
                           ctx.Data = new Dictionary<string, object>()
                           {
                               ["Request"] = request,
                               ["Context"] = context,
                               ["Response"] = response
                           };
                       })
                       .WithException(ex)
                       .Build();

            await DiagnosticStore.Save(entry);

            if (_raiseException) throw ex;

            //response.Status = ActionResultStatus.Error;
            //response.CompletionMiddleware = context.CurrentMiddleware;
            //response.ErrorException =
            //    new ActionException(
            //        ex.Message,
            //        ActionExceptionState.Execution,
            //        context.Get<List<ActionParamDto>>("ParsedParams"),
            //        ex
            //    );
        }
    }
}
