using System;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using NitroSystem.Dnn.BusinessEngine.App.Services.Enums;
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views;
using NitroSystem.Dnn.BusinessEngine.Core.Enums;

namespace NitroSystem.Dnn.BusinessEngine.App.Services.ViewModels.Dashboard
{
    public class DashboardPageViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid DashboardId { get; set; }
        public Guid DashboardModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? ExistingPageId { get; set; }
        public DashboardPageType PageType { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsVisible { get; set; }
        public bool InheritPermissionFromDashboard { get; set; }
        public int ViewOrder { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public bool IsChild { get; set; }
        public IEnumerable<string> AuthorizationViewPage { get; set; }
        public DashboardPageModuleViewModel Module { get; set; }
        public IDictionary<string, object> Settings { get; set; }
        public bool IncludeModule { get { return this.Module != null ? true : false; } }
    }
}