using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_ModuleFieldTypes")]
    [Cacheable("BE_FieldType_View_", CacheItemPriority.Default, 20)]
    public class ModuleFieldTypeView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string FieldType { get; set; }
        public string Title { get; set; }
        public string FieldComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public string FieldJsPath { get; set; }
        public string DirectiveJsPath { get; set; }
        public string GeneratePanesBusinessControllerClass { get; set; }
        public string CustomEvents { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool IsGroupField { get; set; }
        public bool IsContentField { get; set; }
        public bool HasDataSource { get; set; }
        public object DefaultSettings { get; set; }
        public string ValidationPattern { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
        public int GroupViewOrder { get; set; }
    }
}