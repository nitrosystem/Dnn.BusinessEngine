using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.BuildModule.Models
{
    public class ModuleResourceInfo
    {
        public Guid ModuleId { get; set; }
        public string ResourcePath { get; set; }
        public string ResourceType { get; set; }
        public int LoadOrder { get; set; }
    }
}
