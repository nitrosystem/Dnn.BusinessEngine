using System;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module
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