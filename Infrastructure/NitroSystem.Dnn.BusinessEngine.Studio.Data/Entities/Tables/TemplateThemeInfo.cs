using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
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