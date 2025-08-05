using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.DB.Entities
{
    [Table("BusinessEngineBasicExtensions_DataSourceServices")]
    [Cacheable("BEBX_DataSourceServices_", CacheItemPriority.Default, 20)]
    public class DataSourceServiceInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public Guid ViewModelId { get; set; }
        public string StoredProcedureName { get; set; }
        public string BaseQuery { get; set; }
        public string Entities { get; set; }
        public string JoinRelationships { get; set; }
        public string ViewModelProperties { get; set; }
        public string Filters { get; set; }
        public string SortItems { get; set; }
        public bool EnablePaging { get; set; }
        public string PageIndexParam { get; set; }
        public string PageSizeParam { get; set; }
        public string Settings { get; set; }
    }
}
