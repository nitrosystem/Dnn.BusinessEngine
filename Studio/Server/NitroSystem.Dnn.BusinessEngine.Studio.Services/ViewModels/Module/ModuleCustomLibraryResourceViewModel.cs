using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Module
{
    public class ModuleCustomLibraryResourceViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid LibraryId { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}