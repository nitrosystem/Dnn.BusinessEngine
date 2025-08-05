using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database
{
    public class EntityColumnInfo
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public bool IsSelected { get; set; }
        public string ValueType { get; set; }
        public string EntityAliasName { get; set; }
        public string EntityColumnName { get; set; }
        public string Value { get; set; }
    }
}