using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Module
{
    public class PageResourceViewModel
    {
        public bool IsSystemResource { get; set; }
        public bool IsCustomResource { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool IsActive { get; set; }
        public int LoadOrder { get; set; }
    }
}
