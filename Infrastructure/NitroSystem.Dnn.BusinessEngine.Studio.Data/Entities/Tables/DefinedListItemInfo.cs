using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngine_DefinedListItems")]
    [Cacheable("BE_DefinedListItems_", CacheItemPriority.Default, 20)]
    [Scope("ListId")]
    public class DefinedListItemInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public int ItemLevel { get; set; }
        public string ParentValue { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Data { get; set; }
        public int ViewOrder { get; set; }
    }
}