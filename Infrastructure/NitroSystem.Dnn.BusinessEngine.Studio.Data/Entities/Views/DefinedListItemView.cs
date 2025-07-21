using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_DefinedListItems")]
    [Cacheable("BE_DefinedListItems_View_", CacheItemPriority.Default, 20)]
    [Scope("ListId")]
    public class DefinedListItemView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public Guid? FieldId { get; set; }
        public string ListName { get; set; }
        public int ItemLevel { get; set; }
        public string ParentValue { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Data { get; set; }
        public int ViewOrder { get; set; }
    }
}