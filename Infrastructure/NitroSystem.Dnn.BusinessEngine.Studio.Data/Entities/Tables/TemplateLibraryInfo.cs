using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_TemplateLibraries")]
    [Cacheable("BE_TemplateLibraries_", CacheItemPriority.Default, 20)]
    [Scope("TemplateId")]
    public class TemplateLibraryInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string LibraryName { get; set; }
        public string LibraryVersion { get; set; }
        public bool IsInstalled { get; set; }
    }
}