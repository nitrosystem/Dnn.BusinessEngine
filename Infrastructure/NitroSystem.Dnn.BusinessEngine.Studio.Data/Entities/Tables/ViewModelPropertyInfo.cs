using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ViewModelProperties")]
    [Cacheable("BE_ViewModelProperties_", CacheItemPriority.Default, 20)]
    [Scope("ViewModelId")]
    public class ViewModelPropertyInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ViewModelId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public int ViewOrder { get; set; }
    }
}