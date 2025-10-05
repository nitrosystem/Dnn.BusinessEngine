using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_ModuleFieldSettings")]
    [Cacheable("BE_Modules_Fields_Settings_View_", CacheItemPriority.Default, 20)]
    public class ModuleFieldSettingView : IEntity
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public Guid ModuleId { get; set; }
        public string FieldName { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }
}