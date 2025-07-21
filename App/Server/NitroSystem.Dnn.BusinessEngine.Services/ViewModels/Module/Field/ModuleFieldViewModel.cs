using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module.Field
{
    public class ModuleFieldViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string FieldName { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string PaneName { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string ThemeCssClass { get; set; }
        public bool IsSkinTemplate { get; set; }
        public bool IsSkinTheme { get; set; }
        public string FieldType { get; set; }
        public string FieldText { get; set; }
        public bool IsGroup { get; set; }
        public bool IsRequired { get; set; }
        public bool IsShow { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSelective { get; set; }
        public bool IsValuable { get; set; }
        public bool IsJsonValue { get; set; }
        public bool IsNew { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationViewField { get; set; }
        public IEnumerable<ExpressionInfo> ShowConditions { get; set; }
        public IEnumerable<ExpressionInfo> EnableConditions { get; set; }
        public IEnumerable<FieldValueInfo> FieldValues { get; set; }
        public IEnumerable<ActionViewModel> Actions { get; set; }
        public FieldDataSourceResult DataSource { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}