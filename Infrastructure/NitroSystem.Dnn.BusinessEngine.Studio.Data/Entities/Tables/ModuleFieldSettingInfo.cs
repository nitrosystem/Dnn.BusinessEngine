using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleFieldSettings")]
    [Cacheable("BE_ModuleFieldSettings_", CacheItemPriority.Default, 20)]
    [Scope("FieldId")]
    public class ModuleFieldSettingInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}