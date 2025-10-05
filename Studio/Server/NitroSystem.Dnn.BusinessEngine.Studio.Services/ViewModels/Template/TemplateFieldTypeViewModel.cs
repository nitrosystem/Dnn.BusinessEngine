using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Template
{
    public class TemplateFieldTypeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string Icon { get; set; }
    }
}
