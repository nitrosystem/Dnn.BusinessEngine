using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class BuildModuleFieldDto
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string FieldType { get; set; }
        public string FieldName { get; set; }
        public string FieldText { get; set; }
        public string PaneName { get; set; }
        public string Template { get; set; }
        public string Theme { get; set; }
        public string ThemeCssClass { get; set; }
        public bool CanHaveValue  { get; set; }
        public bool IsRequired { get; set; }
        public bool IsGroupField { get; set; }
        public bool IsParent { get; set; }
        public bool HasDataSource { get; set; }
        public FieldDataSourceInfo DataSource { get; set; }
        public string FieldTypeGeneratePanesBusinessControllerClass { get; set; }
        public IEnumerable<ExpressionInfo> ShowConditions { get; set; }
        public IEnumerable<FieldValueInfo> FieldValues { get; set; }
        public IEnumerable<string> AuthorizationViewField { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public ModuleFieldGlobalSettings GlobalSettings { get; set; }
        public int ViewOrder { get; set; }
    }
}
