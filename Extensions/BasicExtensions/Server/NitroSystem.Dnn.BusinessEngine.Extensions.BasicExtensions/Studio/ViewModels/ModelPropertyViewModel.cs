using System;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels
{
    public class ModelPropertyViewModel
    {
        public Guid Id { get; set; }
        public string PropertyName { get; set; }
        public bool IsSelected { get; set; }
        public string ValueType { get; set; }
        public string EntityAliasName { get; set; }
        public string ColumnName { get; set; }
        public string Value { get; set; }
    }
}