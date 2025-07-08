using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard.Skin
{
    public class FieldTypeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string FieldType { get; set; }
        public IEnumerable<FieldTypeTemplateViewModel> Templates { get; set; }
        public IEnumerable<FieldTypeThemeViewModel> Themes { get; set; }
    }
}
