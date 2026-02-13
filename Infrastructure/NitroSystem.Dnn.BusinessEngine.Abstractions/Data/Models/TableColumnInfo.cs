namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Data.Models
{
    public class TableColumnInfo
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string ColumnTypeWithoutSize { get; set; }
        public string DefaultValue { get; set; }
        public int MaxLength { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsIdentity { get; set; }
        public bool AllowNulls { get; set; }
        public bool IsComputed { get; set; }
    }
}
