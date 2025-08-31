using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Engine.Dto
{
    public class ExtensionManifestDto
    {
        public Guid Id { get; set; }
        public string ExtensionName { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionImage { get; set; }
        public string FolderName { get; set; }
        public bool IsNewExtension { get; set; }
        public string VersionType { get; set; }
        public string Version { get; set; }
        public string Summary { get; set; }
        public bool IsCommercial { get; set; }
        public string LicenseType { get; set; }
        public string SourceUrl { get; set; }
    }
}
