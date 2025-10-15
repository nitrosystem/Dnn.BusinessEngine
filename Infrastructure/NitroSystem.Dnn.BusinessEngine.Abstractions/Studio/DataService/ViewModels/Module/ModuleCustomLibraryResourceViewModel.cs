using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module
{
    public class ModuleCustomLibraryResourceViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public ModuleResourceContentType ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}