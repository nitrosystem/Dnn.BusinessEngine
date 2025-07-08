using NitroSystem.Dnn.BusinessEngine.Common.Models;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Entity;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels
{
    [Entity(typeof(EntityInfo))]
    public class EntityViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid? DatabaseId { get; set; }
        public Guid? GroupId { get; set; }
        public string EntityName { get; set; }
        public EntityType EntityType { get; set; }
        public string TableName { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsMultipleColumnsForPK { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public IEnumerable<EntityColumnViewModel> Columns { get; set; }
        public IEnumerable<TableRelationshipInfo> Relationships { get; set; }
    }
}