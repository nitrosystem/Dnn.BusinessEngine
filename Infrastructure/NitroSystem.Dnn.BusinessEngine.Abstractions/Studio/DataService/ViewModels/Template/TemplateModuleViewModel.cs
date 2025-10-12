using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataServices.ViewModels.Template
{
    public class TemplateModuleViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public ModuleType ModuleType { get; set; }
        public string ItemName { get; set; }
        public string ItemImage { get; set; }
        public string TemplatePath { get; set; }
        public int ViewOrder { get; set; }
    }
}
