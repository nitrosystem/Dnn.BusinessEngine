using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Dashboard
{
    public class DashboardSkinLibraryViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
    }
}
