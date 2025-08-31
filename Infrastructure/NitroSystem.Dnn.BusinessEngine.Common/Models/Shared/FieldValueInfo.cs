using NitroSystem.Dnn.BusinessEngine.Common.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Common.Models.Shared
{
    public class FieldValueInfo
    {
        public string ValueExpression { get; set; }
        public IEnumerable<ExpressionInfo> Conditions { get; set; }
    }
}
