using NitroSystem.Dnn.BusinessEngine.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Models.Shared
{
    public class FieldDataSourceResult
    {
        public FieldDataSourceType Type { get; set; }
        public string VariableName { get; set; }
        public string ValueField { get; set; }
        public string TextField { get; set; }
        public IEnumerable<object> Items { get; set; }
    }
}
