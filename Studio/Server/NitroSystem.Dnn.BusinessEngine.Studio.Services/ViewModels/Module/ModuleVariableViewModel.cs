using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Studio.DataServices.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Studio.DataServices.ViewModels.Module
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