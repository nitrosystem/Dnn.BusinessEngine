using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models
{
    public class ModuleFieldValueInfo
    {
        public string ValueExpression { get; set; }
        public IEnumerable<ExpressionInfo> Conditions { get; set; }
    }
}
