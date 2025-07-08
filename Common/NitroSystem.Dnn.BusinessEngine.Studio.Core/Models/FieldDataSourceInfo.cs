using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Models
{
    public class FieldDataSourceInfo
    {
        public Guid? ListId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? ActionId { get; set; }
        public FieldDataSourceType Type { get; set; }
        public string ListName { get; set; }
        public string TextField { get; set; }
        public string ValueField { get; set; }
        public long TotalCount { get; set; }
        public bool? RunServiceClientSide { get; set; }
        public IEnumerable<ParamInfo> ServiceParams { get; set; }
        public IEnumerable<ExpressionInfo> ListFilters { get; set; }
        public IEnumerable<object> Items { get; set; }
    }
}
