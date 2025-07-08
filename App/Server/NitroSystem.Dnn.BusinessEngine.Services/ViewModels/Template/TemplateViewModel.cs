using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    public class TemplateViewModel
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
        public IEnumerable<TemplateThemeInfo> Themes { get; set; }
        public IEnumerable<TemplateFieldTypeViewModel> FieldTypes { get; set; }
        public IEnumerable<TemplateLibraryInfo> Libraries { get; set; }
    }
}