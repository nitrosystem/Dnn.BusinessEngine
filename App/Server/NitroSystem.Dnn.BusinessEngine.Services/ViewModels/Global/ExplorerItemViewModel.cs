using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    public class ExplorerItemViewModel : IViewModel
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Guid? GroupId { get; set; }
        public Guid ScenarioId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? DashboardPageParentId { get; set; }
        public int ViewOrder { get; set; }
    }
}