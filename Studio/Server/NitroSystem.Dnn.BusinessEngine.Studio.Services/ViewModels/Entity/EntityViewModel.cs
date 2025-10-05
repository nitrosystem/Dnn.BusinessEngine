using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Entity
{
    public class EntityViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? GroupId { get; set; }
        public string EntityName { get; set; }
        public string TableName { get; set; }
        public bool IsReadonly { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public EntityType EntityType { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public IEnumerable<EntityColumnViewModel> Columns { get; set; }
    }
}