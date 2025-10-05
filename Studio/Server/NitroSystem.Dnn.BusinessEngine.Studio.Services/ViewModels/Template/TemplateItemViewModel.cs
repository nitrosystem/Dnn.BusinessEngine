using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Template
{
    public class TemplateItemViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string ItemType { get; set; }
        public string ItemSubtype { get; set; }
        public string ItemName { get; set; }
        public string ItemImage { get; set; }
        public string HtmlPath { get; set; }
        public int ViewOrder { get; set; }
    }
}
