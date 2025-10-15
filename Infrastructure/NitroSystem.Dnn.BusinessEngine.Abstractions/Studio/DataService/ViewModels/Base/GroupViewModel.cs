using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.DataService.ViewModels.Base
{
    public class GroupViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid? ScenarioId { get; set; }
        public string GroupType { get; set; }
        public string ObjectType { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool IsSystemGroup { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}
