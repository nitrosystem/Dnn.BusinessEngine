using NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.InstallExtension
{
    public class InstallExtensionRequest
    {
        public string BasePath { get; set; }
        public string ModulePath { get; set; }
        public string ExtensionZipFile { get; set; }
        public ExtensionManifest Manifest { get; set; }
    }
}
