using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using System;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module
{
    [Table("BusinessEngine_ModuleFieldDataSource")]
    [Cacheable("BE_Modules_Fields_DataSource", CacheItemPriority.Default, 20)]
    [Scope("FieldId")]
    public class ModuleFieldDataSourceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        public Guid? ListId { get; set; }
        public int Type { get; set; }
        public int DataStructure { get; set; }
        public string VariableName { get; set; }
        public string TextField { get; set; }
        public string ValueField { get; set; }
    }
}
