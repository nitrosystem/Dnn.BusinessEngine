using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_TemplateItems")]
    [Cacheable("BE_TemplateItems_", CacheItemPriority.Default, 20)]
    [Scope("TemplateId")]
    public class TemplateItemInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string ItemType { get; set; }
        public string ItemSubtype { get; set; }
        public string ItemName { get; set; }
        public string ItemImage { get; set; }
        public string HtmlPath { get; set; }
        public int ViewOrder { get; set; }
    }
}