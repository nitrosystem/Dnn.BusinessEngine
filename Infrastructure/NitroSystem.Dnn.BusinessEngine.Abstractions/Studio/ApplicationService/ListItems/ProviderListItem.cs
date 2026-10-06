using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
   public class ProviderListItem
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string ProviderName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ProviderType ProviderType { get; set; }
    }
}
