using System;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_ExplorerItems")]
    [Scope("ScenarioId")]
    public class ExplorerItemView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Guid? GroupId { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? ParentId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int ViewOrder { get; set; }
    }
}