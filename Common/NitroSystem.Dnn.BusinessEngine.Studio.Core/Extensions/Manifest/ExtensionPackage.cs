using Newtonsoft.Json.Linq;
using NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest.PackageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Extensions.Manifest
{
    public class ExtensionPackage
    {
        public string PackageType { get; set; }
        public string PackageName { get; set; }
        public IEnumerable<ActionTypeInfo> Actions { get; set; }
        public IEnumerable<ServiceTypeInfo> Services { get; set; }
        public IEnumerable<ModuleFieldTypeInfo> Fields { get; set; }
        public IEnumerable<ModuleFieldTypeTemplateInfo> FieldTemplates { get; set; }
        public IEnumerable<ModuleFieldTypeThemeInfo> FieldThemes { get; set; }
        public IEnumerable<DashboardSkinInfo> Skins { get; set; }
        public IEnumerable<PackageModels.LibraryInfo> Libraries { get; set; }
        public IEnumerable<ProviderInfo> Providers { get; set; }
    }
}
