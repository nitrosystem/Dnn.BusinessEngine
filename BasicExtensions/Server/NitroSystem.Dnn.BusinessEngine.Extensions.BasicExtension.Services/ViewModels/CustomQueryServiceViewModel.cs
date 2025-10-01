using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels.Database
{
    public class CustomQueryServiceViewModel : IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public string StoredProcedureName { get; set; }
        public string Query { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
