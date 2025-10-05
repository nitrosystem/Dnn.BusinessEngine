using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Module
{
    public class ModuleCustomLibraryViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }
        public string LocalPath { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public int LoadOrder { get; set; }
        public IEnumerable<ModuleCustomLibraryResourceViewModel> Resources { get; set; }
    }
}