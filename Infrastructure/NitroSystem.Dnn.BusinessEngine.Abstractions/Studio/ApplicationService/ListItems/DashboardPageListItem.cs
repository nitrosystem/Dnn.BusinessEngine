using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ListItems
{
   public class DashboardPageListItem
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string PageName { get; set; }
        public string Title { get; set; }
    }
}
