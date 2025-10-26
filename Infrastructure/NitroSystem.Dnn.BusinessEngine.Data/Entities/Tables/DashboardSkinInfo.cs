using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_DashboardSkins")]
    [Cacheable("BE_Dashboard_Skins_", CacheItemPriority.Default, 20)]
    public class DashboardSkinInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string SkinName { get; set; }
        public string Title { get; set; }
        public string SkinImage { get; set; }
        public string SkinPath { get; set; }
        public string JsFiles { get; set; }
        public string CssFiles { get; set; }
        public string Description { get; set; }
    }
}