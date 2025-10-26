using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_DashboardSkinTemplates")]
    [Cacheable("BE_DashboardSkinTemplates_", CacheItemPriority.Default, 20)]
    [Scope("SkinId")]
    public class DashboardSkinTemplateInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid SkinId { get; set; }
        public int ModuleType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string PreviewImages { get; set; }
        public string JsFiles { get; set; }
        public string CssFiles { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}