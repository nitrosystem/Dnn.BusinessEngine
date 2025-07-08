using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Dapper;
using Dapper.Contrib.Extensions;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Data.RepositoryBase;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_Scenarios")]
    [Cacheable("BE_Scenarios_", CacheItemPriority.Default, 20)]
    public class ScenarioInfo : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string ScenarioName { get; set; }
        public string ScenarioTitle { get; set; }
        public string DatabaseObjectPrefix { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}