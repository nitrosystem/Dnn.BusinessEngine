using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Views
{
    [Table("BusinessEngineView_ExplorerItems")]
    [Scope("ScenarioId")]
    public class ExplorerItemView : IEntity
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