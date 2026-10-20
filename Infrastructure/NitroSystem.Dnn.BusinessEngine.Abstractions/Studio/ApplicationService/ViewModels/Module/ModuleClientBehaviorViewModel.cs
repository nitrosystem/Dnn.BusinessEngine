using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Module
{
    public class ModuleClientBehaviorViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? FieldId { get; set; }
        public Guid? ActionId { get; set; }
        public string Title { get; set; }
        public string Event { get; set; }
        public string Script { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}
