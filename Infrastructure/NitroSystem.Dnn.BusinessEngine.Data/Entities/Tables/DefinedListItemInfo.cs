using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_DefinedListItems")]
    [Cacheable("BE_DefinedListItems_", CacheItemPriority.Default, 20)]
    [Scope("ListId")]
    public class DefinedListItemInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public int ViewOrder { get; set; }
    }
}