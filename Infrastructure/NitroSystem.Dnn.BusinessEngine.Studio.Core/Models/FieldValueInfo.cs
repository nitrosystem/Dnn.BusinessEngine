using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.Models
{
    public class FieldValueInfo
    {
        public string ValueExpression { get; set; }
        public IEnumerable<ExpressionInfo> Conditions { get; set; }
    }
}
