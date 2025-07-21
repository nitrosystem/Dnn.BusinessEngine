using NitroSystem.Dnn.BusinessEngine.Studio.Services.Mapping;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class VariableTypeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string VariableType { get; set; }
        public VariableTypeLanguage Language { get; set; }
        public string Category { get; set; }
        public bool IsSystemVariable { get; set; }
        public bool SupportCsharp { get; set; }
        public bool SupportSql { get; set; }
        public bool SupportJs { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
