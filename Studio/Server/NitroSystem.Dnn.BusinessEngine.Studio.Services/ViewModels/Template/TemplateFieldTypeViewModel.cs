using System;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Template
{
    public class TemplateFieldTypeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public string Icon { get; set; }
    }
}
