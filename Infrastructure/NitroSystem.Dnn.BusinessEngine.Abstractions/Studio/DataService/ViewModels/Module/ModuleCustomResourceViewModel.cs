using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Module
{
    public class ModuleCustomResourceViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid LibraryId { get; set; }
        public string AddressType { get; set; }
        public ResourceContentType ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}