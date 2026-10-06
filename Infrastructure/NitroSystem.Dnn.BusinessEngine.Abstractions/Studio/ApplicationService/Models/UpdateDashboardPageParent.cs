using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Models
{
    public class UpdateDashboardPageParent
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
    }
}
