using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Template
{
    public class TemplateViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateImage { get; set; }
        public string TemplatePath { get; set; }
        public string PreviewImages { get; set; }
        public bool IsFree { get; set; }
        public bool IsDisabled { get; set; }
        public byte Rate { get; set; }
        public decimal PaidAmount { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<TemplateThemeViewModel> Themes { get; set; }
        public IEnumerable<TemplateItemViewModel> Items { get; set; }
        public IEnumerable<TemplateFieldTypeViewModel> FieldTypes { get; set; }
    }
}