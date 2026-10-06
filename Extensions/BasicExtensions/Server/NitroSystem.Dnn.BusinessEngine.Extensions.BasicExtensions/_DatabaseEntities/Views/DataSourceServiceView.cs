using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.DatabaseEntities.Views
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
    }
}
