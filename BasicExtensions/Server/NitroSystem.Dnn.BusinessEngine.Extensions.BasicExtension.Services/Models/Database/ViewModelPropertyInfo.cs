using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database
{
    public class ViewModelPropertyInfo
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