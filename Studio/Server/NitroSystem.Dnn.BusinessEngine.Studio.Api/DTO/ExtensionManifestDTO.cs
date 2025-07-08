using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Api.DTO
{
    public class ExtensionManifestDTO
    {
        public Guid? ExtensionId { get; set; }
        public Guid InstallTemporaryItemId { get; set; }
        public string ExtensionName { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionImage { get; set; }
        public string FolderName { get; set; }
        public string VersionType { get; set; }
        public string Version { get; set; }
        public string Summary { get; set; }
        public bool IsCommercial { get; set; }
        public string LicenseType { get; set; }
        public string SourceUrl { get; set; }
    }
}
