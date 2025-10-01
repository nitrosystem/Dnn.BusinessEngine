using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
{
    public class ModuleDto 
    {
        public Guid Id { get; set; }
        public int PortalId { get; set; }
        public int? DnnModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public int Version { get; set; }
        public ModuleWrapper Wrapper { get; set; }
        public ModuleType ModuleType { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}