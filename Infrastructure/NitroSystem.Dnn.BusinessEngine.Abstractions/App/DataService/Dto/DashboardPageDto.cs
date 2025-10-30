using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Enums;
using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.App.DataService.Dto
{
   public class DashboardPageDto
    {
        public Guid Id { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsVisible { get; set; }
        public bool InheritPermissionFromDashboard { get; set; }
        public int ViewOrder { get; set; }
        public IEnumerable<string> AuthorizationViewPage { get; set; }
        public IEnumerable<DashboardPageDto> Pages { get; set; }
        public DashboardPageType PageType { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
