using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Views
{
    [Table("BusinessEngineView_Modules")]
    [Cacheable("BE_Modules_View_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class ModuleView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public int PortalId { get; set; }
        public int? DnnModuleId { get; set; }
        public int ModuleType { get; set; }
        public int Wrapper { get; set; }
        public string ScenarioName { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public bool IsSSR { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string LayoutTemplate { get; set; }
        public string LayoutCss { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int Version { get; set; }
        public int ViewOrder { get; set; }
    }
}