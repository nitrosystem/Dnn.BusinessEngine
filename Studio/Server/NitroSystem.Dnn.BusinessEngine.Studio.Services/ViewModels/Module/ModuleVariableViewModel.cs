using System;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Module
{
    public class ModuleVariableViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? AppModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public ModuleVariableScope Scope { get; set; }
    }
}