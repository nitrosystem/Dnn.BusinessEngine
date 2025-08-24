using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleFields")]
    [Cacheable("BE_ModuleFields_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleFieldInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
        public string FieldValueProperty { get; set; }
        public string PaneName { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool HasDataSource { get; set; }
        public string DataSource { get; set; }
        public string ShowConditions { get; set; }
        public string ConditionalValues { get; set; }
        public string AuthorizationViewField { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}