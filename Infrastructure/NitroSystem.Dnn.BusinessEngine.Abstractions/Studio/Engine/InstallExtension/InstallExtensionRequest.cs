using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension
{
    public class InstallExtensionRequest
    {
        public string BasePath { get; set; }
        public string ModulePath { get; set; }
        public string ExtensionZipFile { get; set; }
        public ExtensionManifest Manifest { get; set; }
    }
}
