namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models
{
    public sealed class TaskResourceUsage
    {
        public long ElapsedMs { get; set; }
        public double CpuMs { get; set; }
        public long MemoryDeltaBytes { get; set; }
    }
}
