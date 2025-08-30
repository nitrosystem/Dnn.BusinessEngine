using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration
{
    public static class ModelDefinitionJson
    {
        public static string ToJson(NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration.ModelDefinition def)
        => JsonConvert.SerializeObject(def, Formatting.Indented);

        public static NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration.ModelDefinition FromJson(string json)
        => JsonConvert.DeserializeObject<NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration.ModelDefinition>(json);
    }
}
