using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard
{
    public class DashboardSkinViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string SkinName { get; set; }
        public string Title { get; set; }
        public string SkinImage { get; set; }
        public string SkinPath { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> CssFiles { get; set; }
        public IEnumerable<string> JsFiles { get; set; }
        public IEnumerable<DashboardSkinTemplateViewModel> Templates { get; set; }
    }
}
