using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Module
{
    public class ModuleViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public int PortalId { get; set; }
        public int? SiteModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int Version { get; set; }
        public int ViewOrder { get; set; }
        public ModuleType ModuleType { get; set; }
        public ModuleWrapper Wrapper { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}