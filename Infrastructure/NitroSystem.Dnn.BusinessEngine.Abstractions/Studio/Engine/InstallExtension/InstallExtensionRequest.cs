using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension
{
    public class InstallExtensionRequest
    {
        public string BasePath { get; set; }
        public string ModulePath { get; set; }
        public string ExtractPath { get; set; }
        public bool IsAvailableExtension { get; set; }
        public string AvailableExtensionFileName { get; set; }
        public string Channel { get; set; }
        public ExtensionManifest Manifest { get; set; }
    }
}
