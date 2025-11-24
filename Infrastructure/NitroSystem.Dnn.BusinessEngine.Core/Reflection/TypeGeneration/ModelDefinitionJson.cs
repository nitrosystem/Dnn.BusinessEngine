using Newtonsoft.Json;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration
{
    public static class ModelDefinitionJson
    {
        public static string ToJson(ModelDefinition def)
        => JsonConvert.SerializeObject(def, Formatting.Indented);

        public static ModelDefinition FromJson(string json)
        => JsonConvert.DeserializeObject<ModelDefinition>(json);
    }
}
