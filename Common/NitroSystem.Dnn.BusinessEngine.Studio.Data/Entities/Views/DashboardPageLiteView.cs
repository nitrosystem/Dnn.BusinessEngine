using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_DashboardPagesLite")]
    [Cacheable("BE_DashboardPagesLite_View_", CacheItemPriority.Default, 20)]
    [Scope("DashboardModuleId")]
    public class DashboardPageLiteView : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid DashboardId { get; set; }
        public Guid DashboardModuleId { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
    }
}
