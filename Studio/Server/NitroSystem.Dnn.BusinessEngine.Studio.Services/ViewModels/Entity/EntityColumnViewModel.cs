using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Entity
{
    public class EntityColumnViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public bool AllowNulls { get; set; }
        public string DefaultValue { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public bool IsComputedColumn { get; set; }
        public bool IsIdentity { get; set; }
        public string Formula { get; set; }
        public string Settings { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
    }
}