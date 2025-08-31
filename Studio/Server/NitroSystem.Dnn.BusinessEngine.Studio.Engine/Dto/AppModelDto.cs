using NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
   public class AppModelDto
    {
        public string ModelName { get; set; }
        public string ScenarioName { get; set; }
        public List<PropertyDefinition> Properties { get; set; }
    }
}
