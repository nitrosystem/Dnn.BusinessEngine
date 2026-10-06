using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_DefinedListItems")]
    [Cacheable("BE_DefinedLists_Items_View_", CacheItemPriority.Default, 20)]
    [Scope("ListId")]
    public class DefinedListItemView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public string ListName { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public int ViewOrder { get; set; }
    }
}