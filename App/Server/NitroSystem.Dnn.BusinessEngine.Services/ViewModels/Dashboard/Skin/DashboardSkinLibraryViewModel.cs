using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Dashboard.Skin
{
    public class DashboardSkinLibraryViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
    }
}
