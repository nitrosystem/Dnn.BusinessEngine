using System;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database.SubmitEntity
{
    public class ColumnInfo
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public bool IsSelected { get; set; }
    }
}