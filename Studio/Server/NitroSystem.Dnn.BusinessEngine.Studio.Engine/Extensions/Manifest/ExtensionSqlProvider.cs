using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest
{
    public class ExtensionSqlProvider
    {
        public SqlProviderType Type { get; set; }
        public string File { get; set; }
        public string Version { get; set; }
    }
}
