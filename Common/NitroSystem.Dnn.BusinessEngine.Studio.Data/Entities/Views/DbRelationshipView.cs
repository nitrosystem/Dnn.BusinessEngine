
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_DbRelationships")]
    [Cacheable("BE_DbRelationships_View_", CacheItemPriority.Default, 20)]
    public class DbRelationshipView : IEntity
    {
        public Guid Id { get; set; }
        public int ItemId { get; set; }
        public string ParentTable { get; set; }
        public string ChildTable { get; set; }
        public string ParentColumn { get; set; }
        public string ChildColumn { get; set; }
        public string ForeignKeyName { get; set; }
    }
}
