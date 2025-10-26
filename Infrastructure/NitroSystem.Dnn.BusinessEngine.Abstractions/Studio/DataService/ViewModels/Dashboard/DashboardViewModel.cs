using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Services.ViewModels.Dashboard
{
    public class DashboardViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public Guid ModuleId { get; set; }
        public int PortalId { get; set; }
        public int DnnModuleId { get; set; }
        public string ModuleName { get; set; }
        public string ModuleTitle { get; set; }
        public string Skin { get; set; }
        public string Template { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationViewDashboard { get; set; }
    }
}