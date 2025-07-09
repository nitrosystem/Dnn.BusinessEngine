using NitroSystem.Dnn.BusinessEngine.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Models
{
  public  class FieldDataSourceResult
    {
        public FieldDataSourceType Type { get; set; }
        public IEnumerable<object> Items { get; set; }
        public int TotalCount { get; set; }
        public string TextField { get; set; }
        public string ValueField { get; set; }
        public string ErrorMessage { get; set; }
        internal string DataSourceJson { get; set; }
    }
}
