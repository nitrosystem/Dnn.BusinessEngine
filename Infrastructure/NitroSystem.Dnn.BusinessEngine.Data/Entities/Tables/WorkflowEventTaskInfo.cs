using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_WorkflowEventTasks")]
    [Cacheable("BE_WorkflowEventTasks_", CacheItemPriority.Default, 20)]
    [Scope("WorkflowName")]
    public class WorkflowEventTaskInfo : IEntity
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string WorkflowName { get; set; }
        public string EventName { get; set; }
        public string StepName { get; set; }
        public string TaskName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsError { get; set; }
        public string ExceptionMessage { get; set; }
        public long ElapsedMs { get; set; }
        public double CpuMs { get; set; }
        public long MemoryDeltaBytes { get; set; }
    }
}