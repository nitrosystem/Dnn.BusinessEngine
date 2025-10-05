using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto
{
    public class ResourceDto
    {
        public Guid ModuleId { get; set; }
        public ResourceType ResourceType { get; set; }
        public ActionType ActionType { get; set; }
        public string EntryType { get; set; }
        public string ResourcePath { get; set; }
        public string Additional { get; set; }
        public string Condition { get; set; }
        public string CacheKey { get; set; }
        public int LoadOrder { get; set; }
    }
}
