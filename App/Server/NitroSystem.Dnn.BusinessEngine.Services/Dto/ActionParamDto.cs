using System;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.Dto
{
    public class ActionParamDto
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public object ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
