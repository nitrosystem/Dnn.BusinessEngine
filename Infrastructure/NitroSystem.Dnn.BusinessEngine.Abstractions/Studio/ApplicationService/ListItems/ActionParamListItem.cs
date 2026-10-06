using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
    public class ActionParamListItem
    {
        public Guid Id { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public int ViewOrder { get; set; }
    }
}
