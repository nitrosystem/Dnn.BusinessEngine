using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_TemplateThemes")]
    [Cacheable("BE_TemplateThemes_", CacheItemPriority.Default, 20)]
    [Scope("TemplateId")]
    public class TemplateThemeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeCssPath { get; set; }
        public string ThemeCssClass { get; set; }
        public int ViewOrder { get; set; }
    }
}