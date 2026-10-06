using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models
{
    public class ExtensionSqlProvider
    {
        public SqlProviderType Type { get; set; }
        public string File { get; set; }
        public string Version { get; set; }
    }
}
