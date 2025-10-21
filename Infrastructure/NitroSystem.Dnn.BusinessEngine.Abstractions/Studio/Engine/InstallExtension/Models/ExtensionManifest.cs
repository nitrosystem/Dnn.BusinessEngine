using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models
{
   public class ExtensionManifest
    {
        public Guid? Id { get; set; }
        public string ExtensionName { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionImage { get; set; }
        public bool IsNewExtension { get; set; }
        public string FolderName { get; set; }
        public string VersionType { get; set; }
        public string Version { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string ReleaseNotes { get; set; }
        public bool IsCommercial { get; set; }
        public string LicenseType { get; set; }
        public string LicenseKey { get; set; }
        public string SourceUrl { get; set; }
        public ExtensionOwner Owner { get; set; }
        public IEnumerable<ExtensionResource> Resources { get; set; }
        public IEnumerable<ExtensionAssembly> Assemblies { get; set; }
        public IEnumerable<ExtensionSqlProvider> SqlProviders { get; set; }
    }
}
