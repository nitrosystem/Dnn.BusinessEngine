using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Repository;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_Services")]
    [Cacheable("BE_View_Services_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class ServiceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string ServiceSubtype { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasResult { get; set; }
        public byte ResultType { get; set; }
        public string AuthorizationRunService { get; set; }
        public string Settings { get; set; }
        public string ServiceTypeIcon { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int Version { get; set; }
        public int ViewOrder { get; set; }
    }
}