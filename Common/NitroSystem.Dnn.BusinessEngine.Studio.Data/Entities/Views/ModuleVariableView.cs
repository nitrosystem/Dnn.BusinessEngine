using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Tables
{
    [Table("BusinessEngineView_ModuleVariables")]
    [Cacheable("BE_ModuleVariables_Views_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleVariableView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ViewModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public int Language { get; set; }
        public int Scope { get; set; }
        public bool IsSystemVariable { get; set; }
        public string Category { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}