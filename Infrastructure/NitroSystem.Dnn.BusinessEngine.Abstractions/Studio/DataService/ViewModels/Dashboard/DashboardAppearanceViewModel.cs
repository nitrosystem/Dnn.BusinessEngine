using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard
{
    public class DashboardAppearanceViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? SkinId { get; set; }
        public string Skin { get; set; }
        public string SkinImage { get; set; }
        public string Template { get; set; }
        public string TemplatePath { get; set; }
        public string TemplateImage { get; set; }
        public string Theme { get; set; }
    }
}
