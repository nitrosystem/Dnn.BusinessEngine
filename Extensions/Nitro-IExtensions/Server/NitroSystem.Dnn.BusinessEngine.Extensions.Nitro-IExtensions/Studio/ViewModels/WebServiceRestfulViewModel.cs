using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.WebServiceRestful;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.ViewModels
{
  public  class WebServiceRestfulViewModel : IExtensionServiceViewModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? HeaderAppModelId { get; set; }
        public Guid? BodyAppModelId { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string BodyType { get; set; }
        public string BodyRaw { get; set; }
        public IEnumerable<ParamInfo> Params { get; set; }
        public AuthorizationInfo Authorization { get; set; }
        public IEnumerable<ParamInfo> Headers { get; set; }
        public IEnumerable<ParamInfo> BodyFormData { get; set; }
    }
}
