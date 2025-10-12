using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Action
{
    public class ActionParamViewModel
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
