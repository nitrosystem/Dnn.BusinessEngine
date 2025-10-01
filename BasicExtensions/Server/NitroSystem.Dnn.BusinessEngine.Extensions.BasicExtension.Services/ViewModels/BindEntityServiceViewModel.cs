using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels
{
    public class BindEntityServiceViewModel : IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public Guid AppModelId { get; set; }
        public string EntityTableName { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public IEnumerable<ModelPropertyInfo> ModelProperties { get; set; }
        public IEnumerable<FilterItemInfo> Filters { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}