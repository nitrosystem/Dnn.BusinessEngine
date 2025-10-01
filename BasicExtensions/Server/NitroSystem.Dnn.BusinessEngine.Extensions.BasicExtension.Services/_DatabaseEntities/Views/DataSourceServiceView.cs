using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DatabaseEntities.Views
{
    [Table("BusinessEngineBasicExtensionsView_DataSourceServices")]
    [Cacheable("BEBX_DataSourceServices_View_", CacheItemPriority.Default, 20)]
    public class DataSourceServiceView : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string StoredProcedureName { get; set; }
        public bool EnablePaging { get; set; }
        public string TypeRelativePath { get; set; }
        public string TypeFullName { get; set; }
        public string ScenarioName { get; set; }
    }
}
