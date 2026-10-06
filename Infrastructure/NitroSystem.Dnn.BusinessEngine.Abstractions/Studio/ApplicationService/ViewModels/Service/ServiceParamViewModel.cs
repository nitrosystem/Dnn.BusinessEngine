using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Service
{
    public class ServiceParamViewModel
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public int ViewOrder { get; set; }
    }
}
