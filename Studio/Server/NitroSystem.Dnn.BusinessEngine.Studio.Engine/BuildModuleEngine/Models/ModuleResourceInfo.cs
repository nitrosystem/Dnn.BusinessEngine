using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Models
{
    public class ModuleResourceInfo
    {
        public Guid ModuleId { get; set; }
        public string ResourcePath { get; set; }
        public string ResourceType { get; set; }
        public int LoadOrder { get; set; }
    }
}
