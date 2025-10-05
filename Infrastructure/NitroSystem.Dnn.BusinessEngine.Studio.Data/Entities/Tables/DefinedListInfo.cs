using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_DefinedLists")]
    [Cacheable("BE_DefinedLists_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class DefinedListInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ScenarioId { get; set; }
        public string ListName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}