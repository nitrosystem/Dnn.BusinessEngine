using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Workflow.Models
{
    public sealed class MethodProfileResult<T> : TaskInfo
    {
        public string Name { get; set; }
        public T Result { get; set; }
    }
}
