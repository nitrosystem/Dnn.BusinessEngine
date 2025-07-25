using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_Libraries")]
    [Cacheable("BE_Libraries_", CacheItemPriority.Default, 20)]
    public class LibraryInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string Type { get; set; }
        public string LibraryName { get; set; }
        public string Logo { get; set; }
        public string Summary { get; set; }
        public string Version { get; set; }
        public string LocalPath { get; set; }
        public bool IsSystemLibrary { get; set; }
        public bool IsCDN { get; set; }
        public bool IsCommercial { get; set; }
        public bool IsOpenSource { get; set; }
        public bool IsStable { get; set; }
        public string LicenseJson { get; set; }
        public string GithubPage { get; set; }
    }
}