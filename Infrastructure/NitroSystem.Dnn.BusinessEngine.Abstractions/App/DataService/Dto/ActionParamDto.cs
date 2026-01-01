using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto
{
    public class ActionParamDto
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public ValueAssignmentMode ValueAssignmentMode { get; set; }
        public object ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
