using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models
{
    public class ExtensionAssembly
    {
        public string BasePath { get; set; }
        public IEnumerable<string> Items { get; set; }
    }
}