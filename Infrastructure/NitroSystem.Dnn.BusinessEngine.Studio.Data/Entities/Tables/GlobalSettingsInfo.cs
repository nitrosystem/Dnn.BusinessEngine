using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_GlobalSettings")]
    [Cacheable("BE_GlobalSettings_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class GlobalSettingsInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public string GroupName { get; set; }
        public string SettingName { get; set; }
        public object SettingValue { get; set; }
    }
}