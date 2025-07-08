using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
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
        public Guid? PropertyTypeId { get; set; }
        public int ViewOrder { get; set; }
    }
}