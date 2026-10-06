using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels
{
    public class CustomQueryServiceViewModel : IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? AppModelId { get; set; }
        public string StoredProcedureName { get; set; }
        public string Query { get; set; }
        public ResultType ResultType { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
