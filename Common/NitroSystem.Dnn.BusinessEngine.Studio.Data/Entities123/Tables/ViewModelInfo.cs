using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Dapper.Contrib.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ViewModels")]
    [Cacheable("BE_ViewModels_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioID")]
    public class ViewModelInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string ViewModelName { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}