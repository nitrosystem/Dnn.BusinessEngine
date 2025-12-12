using System;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures
{
   public class GroupItemResult
    {
        public Guid ItemId { get; set; }
        public Guid ScenarioId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int ViewOrder { get; set; }
    }
}
