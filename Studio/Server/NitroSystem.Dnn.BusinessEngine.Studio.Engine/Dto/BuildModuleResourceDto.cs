using NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class BuildModuleResourceDto
    {
        public Guid ModuleId { get; set; }
        public ResourceType ResourceType { get; set; }
        public ActionType ActionType { get; set; }
        public string EntryType { get; set; }
        public string ResourcePath { get; set; }
        public string Additional { get; set; }
        public string Condition { get; set; }
        public string CacheKey { get; set; }
        public int LoadOrder { get; set; }
    }
}
