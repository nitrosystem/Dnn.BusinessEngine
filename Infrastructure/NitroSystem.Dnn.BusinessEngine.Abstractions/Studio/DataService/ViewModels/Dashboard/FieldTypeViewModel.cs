using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard
{
    public class FieldTypeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public IEnumerable<FieldTypeTemplateViewModel> Templates { get; set; }
        public IEnumerable<FieldTypeThemeViewModel> Themes { get; set; }
    }
}
