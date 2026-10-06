using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Module
{
    [Table("BusinessEngine_ModuleFieldTypes")]
    [Cacheable("BE_ModuleFieldTypes_", CacheItemPriority.Default, 20)]
    public class ModuleFieldTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string FieldDomain { get; set; }
        public string FieldType { get; set; }
        public string Title  { get; set; }
        public string FieldComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public string FieldJsPath { get; set; }
        public bool IsGroupField { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool HasDataSource { get; set; }
        public bool IsContentField { get; set; }
        public string DefaultSettings { get; set; }
        public string GeneratePanesBusinessControllerClass { get; set; }
        public bool IsPopular { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}