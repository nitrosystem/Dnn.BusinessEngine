using NitroSystem.Dnn.BusinessEngine.App.Services.Dto;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
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
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
        public string PaneName { get; set; }
        public string FieldValueProperty { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool HasDataSource { get; set; }
        public FieldDataSourceResult DataSource { get; set; }
        public string ShowConditions { get; set; }
        public IEnumerable<FieldValueInfo> ConditionalValues { get; set; }
        public IEnumerable<string> AuthorizationViewField { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}