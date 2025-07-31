using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_ServiceTypes")]
    [Cacheable("BE_ServiceType_View_", CacheItemPriority.Default, 20)]
    public class ServiceTypeView : IEntity
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string ServiceDomain { get; set; }
        public string ServiceType { get; set; }
        public string Title { get; set; }
        public string ServiceComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public string BusinessControllerClass { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public int GroupViewOrder { get; set; }
        public int ViewOrder { get; set; }
    }
}