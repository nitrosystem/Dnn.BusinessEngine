using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels
{
    public class SubmitEntityServiceViewModel: IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid EntityId { get; set; }
        public string BaseQuery { get; set; }
        public string InsertBaseQuery { get; set; }
        public string UpdateBaseQuery { get; set; }
        public string StoredProcedureName { get; set; }
        public string CustomQuery{ get; set; }
        public SubmitEntityActionType ActionType { get; set; }
        public SubmitEntity.EntityViewModel Entity { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}