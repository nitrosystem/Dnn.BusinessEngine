using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard
{
    public class FieldTypeTemplateViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string Description { get; set; }
    }
}
