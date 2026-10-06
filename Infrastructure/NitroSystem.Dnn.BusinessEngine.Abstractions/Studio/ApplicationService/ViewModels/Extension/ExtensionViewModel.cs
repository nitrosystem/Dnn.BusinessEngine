using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Extension
{
   public class ExtensionViewModel
    {
        public Guid Id { get; set; }
        public int ExtensionType { get; set; }
        public string ExtensionName { get; set; }
        public string Title { get; set; }
        public string FolderName { get; set; }
        public string Version { get; set; }
        public string Logo { get; set; }
        public string Owner { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
    }
}
