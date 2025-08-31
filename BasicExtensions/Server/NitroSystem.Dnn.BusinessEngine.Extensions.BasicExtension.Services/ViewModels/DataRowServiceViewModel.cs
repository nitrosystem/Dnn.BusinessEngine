using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database
{
    public class DataRowServiceViewModel: IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid AppModelId { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public IEnumerable<EntityInfo> Entities { get; set; }
        public IEnumerable<EntityJoinRelationInfo> JoinRelationships { get; set; }
        public IEnumerable<ModelPropertyInfo> ModelProperties { get; set; }
        public IEnumerable<FilterItemInfo> Filters { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}