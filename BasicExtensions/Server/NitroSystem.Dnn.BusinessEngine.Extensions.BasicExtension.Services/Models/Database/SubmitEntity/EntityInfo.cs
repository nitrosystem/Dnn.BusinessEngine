using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database.SubmitEntity
{
    public class EntityInfo
    {
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string PrimaryKeyParam { get; set; }
        public IEnumerable<ColumnInfo> InsertColumns { get; set; }
        public IEnumerable<ColumnInfo> UpdateColumns { get; set; }
        public IEnumerable<ConditionInfo> InsertConditions { get; set; }
        public IEnumerable<ConditionInfo> UpdateConditions { get; set; }
    }
}