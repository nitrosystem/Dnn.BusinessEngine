using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models
{
    public class TaskInfo
    {
        public string EntryId { get; set; }
        public int UserId { get; set; }
        public string WorkflowName { get; set; }
        public string EventName { get; set; }
        public string StepName { get; set; }
        public string TaskName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Exception Exception { get; set; }
        public bool IsError { get; set; }
        public TaskResourceUsage ResourceUsage { get; set; }
    }
}
