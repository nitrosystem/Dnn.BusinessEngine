using NitroSystem.Dnn.BusinessEngine.Studio.Services.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels
{
    public class ModuleVariableViewModel
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? ViewModelId { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public ModuleVariableScope Scope { get; set; }
        public string ScopeName { get { return Enum.GetName(Scope.GetType(), Scope); } }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public AppModelViewModel ViewModel { get; set; }
    }
}