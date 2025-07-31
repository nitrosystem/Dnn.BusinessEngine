
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ServiceTypes")]
    [Cacheable("BE_ServiceTypes_", CacheItemPriority.Default, 20)]
    public class ServiceTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public Guid GroupId { get; set; }
        public string ServiceDomain { get; set; }
        public string ServiceType { get; set; }
        public string Title { get; set; }
        public string ServiceComponent { get; set; }
        public string BusinessControllerClass { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
