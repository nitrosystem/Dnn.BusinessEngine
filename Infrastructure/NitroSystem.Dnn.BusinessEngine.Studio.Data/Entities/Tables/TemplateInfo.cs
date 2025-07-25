using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_Templates")]
    [Cacheable("BE_Templates_", CacheItemPriority.Default, 20)]
    [Scope("ModuleType")]
    public class TemplateInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public int ModuleType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string PreviewImages { get; set; }
        public bool IsFree { get; set; }
        public bool IsDisabled { get; set; }
        public byte Rate { get; set; }
        public decimal PaidAmount { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}