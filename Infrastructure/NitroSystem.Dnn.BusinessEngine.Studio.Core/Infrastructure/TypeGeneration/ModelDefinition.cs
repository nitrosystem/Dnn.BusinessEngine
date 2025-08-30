using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.TypeGeneration
{
    public sealed class ModelDefinition
    {
        public string Namespace { get; set; } = "DynamicModels";
        public string Name { get; set; }
        public string SchemaVersion { get; set; } = "1.0"; // schema of ModelDefinition
        public string ModelVersion { get; set; } = "1.0"; // version of the model itself
        public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();

        public string ComputeStableKey()
        {
            var sb = new StringBuilder();
            sb.Append(Namespace).Append('|').Append(Name).Append('|').Append(ModelVersion);
            foreach (var p in Properties.OrderBy(p => p.Name))
            {
                sb.Append("||").Append(p.Name).Append(':').Append(p.ClrType);
            }
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
