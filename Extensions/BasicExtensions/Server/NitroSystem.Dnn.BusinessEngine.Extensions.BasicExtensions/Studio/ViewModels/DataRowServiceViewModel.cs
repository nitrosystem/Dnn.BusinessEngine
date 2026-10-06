using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels
{
    public class DataRowServiceViewModel: IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid AppModelId { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public IEnumerable<EntityViewModel> Entities { get; set; }
        public IEnumerable<EntityJoinRelationViewModel> JoinRelationships { get; set; }
        public IEnumerable<ModelPropertyViewModel> ModelProperties { get; set; }
        public IEnumerable<FilterItemViewModel> Filters { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}