using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.ImportExport.Export
{
    public class ManifestModel
    {
        public Guid PackageID { get; set; }
        public string PackageType { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public string Description { get; set; }
        public string ScenarioJsonFile { get; set; }
        public string GroupsJsonFile { get; set; }
        public string EntitiesJsonFile { get; set; }
        public string ViewModelsJsonFile { get; set; }
        public string ServicesJsonFile { get; set; }
        public string ActionsJsonFile { get; set; }
        public string ModulesJsonFile { get; set; }
        public string ModuleVariablesJsonFile { get; set; }
        public string ModulesFieldsJsonFile { get; set; }
        public string ModulesFieldsSettingsJsonFile { get; set; }
        public string PageResourcesJsonFile { get; set; }
        public string DashboardsJsonFile { get; set; }
        public string DashboardPagesJsonFile { get; set; }
        public string DashboardPagesModulesJsonFile { get; set; }
        public string DefinedListsJsonFile { get; set; }
        public string DefinedListsItemsJsonFile { get; set; }
        public string[] ExtensionServices { get; set; }
        public string ScenarioFolderZipFile { get; set; }
        public IEnumerable<string> ExtensionsDependency { get; set; }
    }
}
