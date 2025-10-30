using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardAppearance")]
    [Cacheable("BE_DashboardsAppearance_View_", CacheItemPriority.Default, 20)]
    public class DashboardAppearanceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? SkinId { get; set; }
        public string Skin { get; set; }
        public string SkinImage { get; set; }
        public string Template { get; set; }
        public string TemplatePath { get; set; }
        public string TemplateCssPath { get; set; }
        public string TemplateImage { get; set; }
        public string Theme { get; set; }
    }
}