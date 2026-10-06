using System;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels.SubmitEntity
{
    public class ColumnViewModel
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public bool IsSelected { get; set; }
    }
}