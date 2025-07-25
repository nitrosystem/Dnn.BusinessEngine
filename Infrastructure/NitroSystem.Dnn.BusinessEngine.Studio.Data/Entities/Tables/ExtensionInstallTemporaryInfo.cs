using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ExtensionInstallTemporaries")]
    [Cacheable("BE_ExtensionInstallTemporaries_", CacheItemPriority.Default, 20)]
    public class ExtensionInstallTemporaryView : IEntity
    {
        public Guid Id { get; set; }
        public string ExtensionManifestJson { get; set; }
        public string ExtensionInstallUnzipedPath { get; set; }
        public bool IsExtensionInstalled { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}