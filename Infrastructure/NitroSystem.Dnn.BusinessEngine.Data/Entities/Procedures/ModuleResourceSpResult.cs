using System;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Procedures
{
    public class ModuleResourceSpResult
    {
        public Guid ModuleId { get; set; }
        public int ResourceType { get; set; }
        public int ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public string EntryType { get; set; }
        public bool IsContent { get; set; }
        public string Content { get; set; }
        public int LoadOrder { get; set; }
    }
}
