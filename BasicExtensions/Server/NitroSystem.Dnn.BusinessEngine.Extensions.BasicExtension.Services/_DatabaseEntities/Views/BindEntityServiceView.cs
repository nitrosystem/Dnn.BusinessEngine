using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Views
{
    [Table("BusinessEngineBasicExtensionsView_BindEntityServices")]
    [Cacheable("BEBX_BindEntityServices_View_", CacheItemPriority.Default, 20)]
    public class BindEntityServiceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string StoredProcedureName { get; set; }
        public string TypeRelativePath { get; set; }
        public string TypeFullName { get; set; }
        public string ScenarioName { get; set; }
    }
}
