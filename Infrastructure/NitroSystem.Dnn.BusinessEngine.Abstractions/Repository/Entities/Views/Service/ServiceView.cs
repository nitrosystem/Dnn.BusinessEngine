using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Views.Service
{
    [Table("BusinessEngineView_Services")]
    [Cacheable("BE_Services_View_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class ServiceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasResult { get; set; }
        public byte ResultType { get; set; }
        public string AuthorizationRunService { get; set; }
        public string Settings { get; set; }
        public string ServiceTypeTitle { get; set; }
        public string ServiceComponent { get; set; }
        public string ServiceTypeIcon { get; set; }
        public string ServiceTypeDescription { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserDisplayName { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public string LastModifiedByUserDisplayName { get; set; }
        public int ViewOrder { get; set; }
    }
}