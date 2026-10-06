using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Template
{
    [Table("BusinessEngine_Templates")]
    [Cacheable("BE_Templates_", CacheItemPriority.Default, 20)]
    [Scope("ModuleType")]
    public class TemplateInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public Guid? ParentId { get; set; }
        public int? ModuleType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string TemplateCssPath { get; set; }
        public string PreviewImages { get; set; }
        public string Description { get; set; }
    }
}