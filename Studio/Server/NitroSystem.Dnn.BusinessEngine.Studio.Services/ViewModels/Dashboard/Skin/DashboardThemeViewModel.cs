using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard.Skin
{
    public class DashboardThemeViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string ThemeName { get; set; }
        public string ThemeImage { get; set; }
        public string ThemeCssPath { get; set; }
        public string ThemeCssClass { get; set; }
        public string Description { get; set; }
    }
}
