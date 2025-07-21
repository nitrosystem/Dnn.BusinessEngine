using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_Extensions")]
    [Cacheable("BE_Extensions_", CacheItemPriority.Default, 20)]
    public class ExtensionInfo : IEntity
    {
        public Guid Id { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionName { get; set; }
        public string ExtensionImage { get; set; }
        public string FolderName { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string ReleaseNotes { get; set; }
        public string Owner { get; set; }
        public string Resources { get; set; }
        public string Assemblies { get; set; }
        public string SqlProviders { get; set; }
        public bool IsCommercial { get; set; }
        public string LicenseType { get; set; }
        public string LicenseKey { get; set; }
        public string SourceUrl { get; set; }
        public string VersionType { get; set; }
        public string Version { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}