using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleFieldTypeThemes")]
    [Cacheable("BE_ModuleFieldTypeThemes_", CacheItemPriority.Default, 20)]
    [Scope("FieldType")]
    public class ModuleFieldTypeThemeInfo : IEntity
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string TemplateName { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeCssPath { get; set; }
        public string ThemeCssClass { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}