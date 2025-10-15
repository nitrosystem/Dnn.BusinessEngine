using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Service
{
    public class ServiceParamViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public string ParamType { get; set; }
        public int ViewOrder { get; set; }
    }
}
