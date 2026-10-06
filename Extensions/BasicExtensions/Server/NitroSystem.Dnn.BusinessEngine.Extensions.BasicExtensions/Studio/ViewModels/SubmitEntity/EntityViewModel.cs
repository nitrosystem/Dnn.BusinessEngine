using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels.SubmitEntity
{
    public class EntityViewModel
    {
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public string PrimaryKeyParam { get; set; }
        public IEnumerable<ColumnViewModel> InsertColumns { get; set; }
        public IEnumerable<ColumnViewModel> UpdateColumns { get; set; }
        public IEnumerable<ConditionViewModel> InsertConditions { get; set; }
        public IEnumerable<ConditionViewModel> UpdateConditions { get; set; }
    }
}