using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModuleEngine.Dto
{
    public class BuildModuleDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string ModuleType { get; set; }
        public string ModuleName { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public string BuildPath { get; set; }
    }
}
