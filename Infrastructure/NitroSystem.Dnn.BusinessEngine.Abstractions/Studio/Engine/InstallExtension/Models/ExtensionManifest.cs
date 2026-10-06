using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models
{
   public class ExtensionManifest
    {
        public Guid Id { get; set; }
        public int ExtensionType { get; set; }
        public string ExtensionName { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public string Owner { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public IEnumerable<ExtensionResource> Resources { get; set; }
        public IEnumerable<ExtensionAssembly> Assemblies { get; set; }
        public IEnumerable<ExtensionSqlProvider> SqlProviders { get; set; }
    }
}
