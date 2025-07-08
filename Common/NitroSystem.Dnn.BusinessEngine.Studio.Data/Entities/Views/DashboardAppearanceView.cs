using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardAppearance")]
    [Cacheable("BE_DashboardsAppearance_View_", CacheItemPriority.Default, 20)]
    public class DashboardAppearanceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? SkinId { get; set; }
        public int DashboardType { get; set; }
        public string Skin { get; set; }
        public string SkinPath { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
    }
}