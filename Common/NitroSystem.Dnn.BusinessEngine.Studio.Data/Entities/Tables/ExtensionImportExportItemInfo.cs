using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_ExtensionImportExportItems")]
    [Cacheable("BE_ExtensionExportItems_", CacheItemPriority.Default, 20)]
    public class ExtensionImportExportItemInfo : IEntity
    {
        public Guid Id { get; set; }
        public string ExtensionName { get; set; }
        public int OperationType { get; set; }
        public string ItemType { get; set; }
        public string BusinessControllerClass { get; set; }
        public string MethodName { get; set; }
    }
}