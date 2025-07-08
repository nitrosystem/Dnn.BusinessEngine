using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
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
        public string Description { get; set; }
    }
}