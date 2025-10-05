using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.BuildModuleEngine.Dto
{
    public class ModuleDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string ModuleName { get; set; }
        public string LayoutTemplate { get; set; }
        public IEnumerable<ModuleFieldDto> Fields { get; set; }
        public IEnumerable<ModuleResourceDto> Resources { get; set; }
        public IEnumerable<ModuleExternalResourceDto> ExternalResources { get; set; }
    }
}
