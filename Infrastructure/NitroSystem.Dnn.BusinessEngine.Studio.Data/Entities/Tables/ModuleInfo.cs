using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_Modules")]
    [Cacheable("BE_Modules_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class ModuleInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public int PortalId { get; set; }
        public int? DnnModuleId { get; set; }
        public int ModuleType { get; set; }
        public int ModuleBuilderType { get; set; }
        public int Wrapper { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string PreloadingTemplate { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public bool IsSSR { get; set; }
        public string Settings { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}