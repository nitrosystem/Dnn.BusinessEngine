using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleFieldTypes")]
    [Cacheable("BE_ModuleFieldTypes_", CacheItemPriority.Default, 20)]
    public class ModuleFieldTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public Guid GroupId { get; set; }
        public string FieldType { get; set; }
        public string Title  { get; set; }
        public string FieldComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public string FieldJsPath { get; set; }
        public string DirectiveJsPath { get; set; }
        public string GeneratePanesBusinessControllerClass { get; set; }
        public string CustomEvents { get; set; }
        public bool IsGroupField { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool HasDataSource { get; set; }
        public bool IsContentField { get; set; }
        public object DefaultSettings { get; set; }
        public string ValidationPattern { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}