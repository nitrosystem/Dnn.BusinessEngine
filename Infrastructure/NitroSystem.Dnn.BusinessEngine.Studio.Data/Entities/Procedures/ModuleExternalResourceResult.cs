using System;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    public class ModuleExternalResourceResult
    {
        public Guid ModuleId { get; set; }
        public int ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public string LoadOrder { get; set; }
    }
}
