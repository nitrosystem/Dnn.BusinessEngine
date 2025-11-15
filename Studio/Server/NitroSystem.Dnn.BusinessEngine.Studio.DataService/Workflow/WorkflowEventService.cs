using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Dto;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Workflow;
using NitroSystem.Dnn.BusinessEngine.Shared.Mapper;
using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataService.Workflow
{
    public class WorkflowEventService : IWorkflowEventService
    {
        private readonly IRepositoryBase _repository;

        public WorkflowEventService(IRepositoryBase repository)
        {
            _repository = repository;
        }

        public async Task SaveTaskBatchAsync(List<WorkflowEventTaskDto> tasks)
        {
            var list = HybridMapper.MapCollection<WorkflowEventTaskDto, WorkflowEventTaskInfo>(tasks);

            try
            {
                await _repository.BulkInsertAsync<WorkflowEventTaskInfo>(list);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
