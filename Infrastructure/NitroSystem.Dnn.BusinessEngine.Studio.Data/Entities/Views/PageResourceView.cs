
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_PageResources")]
    [Cacheable("BE_PageResources_View_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class PageResourceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public int? DnnPageId { get; set; }
        public int ModuleType { get; set; }
        public bool IsSystemResource { get; set; }
        public bool IsCustomResource { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsActive { get; set; }
        public int LoadOrder { get; set; }
    }
}
