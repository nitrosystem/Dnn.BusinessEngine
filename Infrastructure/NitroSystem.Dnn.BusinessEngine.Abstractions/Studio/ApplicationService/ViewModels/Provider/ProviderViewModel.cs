using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Provider
{
    public class ProviderViewModel
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string ProviderName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ProviderType ProviderType { get; set; }
    }
}
