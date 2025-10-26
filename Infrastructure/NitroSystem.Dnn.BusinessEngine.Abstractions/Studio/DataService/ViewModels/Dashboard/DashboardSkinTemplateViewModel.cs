using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard
{
    public class DashboardSkinTemplateViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public ModuleType ModuleType { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string PreviewImages { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
