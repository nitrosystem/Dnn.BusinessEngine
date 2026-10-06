using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models
{
    public class ExtensionResource
    {
        public ExtensionResourceType Type { get; set; }
        public string BasePath { get; set; }
        public string ZipFile { get; set; }
    }
}
