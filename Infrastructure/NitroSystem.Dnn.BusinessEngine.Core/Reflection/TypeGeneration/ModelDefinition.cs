using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection.TypeGeneration
{
    public sealed class ModelDefinition
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string SchemaVersion { get; set; }
        public string ModelVersion { get; set; }
        public List<IPropertyDefinition> Properties { get; set; }

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
