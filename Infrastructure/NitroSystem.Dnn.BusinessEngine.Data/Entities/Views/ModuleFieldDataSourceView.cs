using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_ModuleFieldDataSource")]
    [Cacheable("BE_Modules_Fields_DataSource_View_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleFieldDataSourceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public Guid? ListId { get; set; }
        public Guid ModuleId { get; set; }
        public int Type { get; set; }
        public string VariableName { get; set; }
        public string TextField { get; set; }
        public string ValueField { get; set; }
    }
}