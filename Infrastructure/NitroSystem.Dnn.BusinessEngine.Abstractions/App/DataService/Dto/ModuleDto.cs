using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataServices.Dto
{
    public class ModuleDto 
    {
        public Guid Id { get; set; }
        public int PortalId { get; set; }
        public int? SiteModuleId { get; set; }
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