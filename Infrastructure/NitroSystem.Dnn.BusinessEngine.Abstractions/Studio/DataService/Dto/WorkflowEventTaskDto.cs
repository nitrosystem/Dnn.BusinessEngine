using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.Dto
{
    public class WorkflowEventTaskDto
    {
        public string EntryId { get; set; }
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
