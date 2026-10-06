using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module
{
    public class ModuleCustomLibraryResourceViewModel
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public ResourceContentType ResourceContentType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}