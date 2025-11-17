using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Workflow;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.BackgroundTaskFramework.Models;
using NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;

namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow
{
    public sealed class SaveTaskBatchJob : IBackgroundTask
    {
        private readonly IWorkflowEventService _workflowEventService;
        private readonly List<TaskInfo> _batch;

        public string TaskId => Guid.NewGuid().ToString();
        public string Name => "WorkflowEvent_SaveTaskBatchJob";

        public SaveTaskBatchJob(IServiceProvider serviceProvider, List<TaskInfo> batch)
        {
            _workflowEventService = serviceProvider.GetRequiredService<IWorkflowEventService>();
            _batch = batch;
        }

        public async Task RunAsync(CancellationToken token, IProgress<ProgressInfo> progress)
        {
            var list = HybridMapper.MapCollection<TaskInfo, WorkflowEventTaskDto>(_batch,
                (src, dest) =>
                {
                    dest.ExceptionMessage = src.Exception?.Message;
                    dest.ElapsedMs = src.ResourceUsage.ElapsedMs;
                    dest.CpuMs = src.ResourceUsage.CpuMs;
                    dest.MemoryDeltaBytes = src.ResourceUsage.MemoryDeltaBytes;
                }).ToList();
            await _workflowEventService.SaveTaskBatchAsync(list).ConfigureAwait(false);
        }
    }
}
