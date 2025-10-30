using System;
using NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard
{
    public class DashboardPageModuleViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string Template { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public ModuleWrapper Wrapper { get; set; }
        public ModuleType ModuleType { get; set; }
    }
}