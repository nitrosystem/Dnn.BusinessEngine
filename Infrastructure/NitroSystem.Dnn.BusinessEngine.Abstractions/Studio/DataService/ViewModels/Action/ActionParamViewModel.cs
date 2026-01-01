using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Action
{
    public class ActionParamViewModel
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public ValueAssignmentMode ValueAssignmentMode { get; set; }
        public string ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
