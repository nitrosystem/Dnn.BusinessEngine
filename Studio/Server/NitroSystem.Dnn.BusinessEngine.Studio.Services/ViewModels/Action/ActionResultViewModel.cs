using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Action
{
    public class ActionResultViewModel
    {
        public Guid Id { get; set; }
        public string LeftExpression { get; set; }
        public string EvalType { get; set; }
        public string RightExpression { get; set; }
        public string GroupName { get; set; }
        public string Conditions { get; set; }
    }
}
