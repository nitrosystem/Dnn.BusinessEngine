using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Base
{
    public class StudioLibraryViewModel
    {
        public Guid Id { get; set; }
        public string LibraryName { get; set; }
        public string Version { get; set; }
        public int LoadOrder { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}