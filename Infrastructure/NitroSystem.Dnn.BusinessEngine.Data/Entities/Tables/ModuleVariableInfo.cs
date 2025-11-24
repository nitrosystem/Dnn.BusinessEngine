using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ModuleVariables")]
    [Cacheable("BE_Modules_Variables_", CacheItemPriority.Default, 20)]
    [Scope("ModuleId")]
    public class ModuleVariableInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? AppModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string DefaultValue { get; set; }
        public int Scope { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}