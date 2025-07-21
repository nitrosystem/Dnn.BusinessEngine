using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_DefinedLists")]
    [Cacheable("BE_DefinedLists_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class DefinedListInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid? ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FieldId { get; set; }
        public string ListName { get; set; }
        public string ListType { get; set; }
        public bool IsMultiLevelList { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}