using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.BuildModule.Dto
{
    public class ModuleResourceDto
    {
        public Guid ModuleId { get; set; }
        public ModuleResourceType ResourceType { get; set; }
        public ResourceContentType ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public string EntryType { get; set; }
        public int LoadOrder { get; set; }
    }
}
