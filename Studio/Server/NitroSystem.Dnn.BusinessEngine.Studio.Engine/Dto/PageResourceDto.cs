using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class PageResourceDto
    {
        public Guid ModuleId { get; set; }
        public int? DnnPageId { get; set; }
        public bool IsCustomResource { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsActive { get; set; }
        public int LoadOrder { get; set; }
    }
}
