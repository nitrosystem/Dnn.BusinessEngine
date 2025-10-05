using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Models;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.ModuleBuilder.Models
{
    public class FieldValueInfo
    {
        public string ValueExpression { get; set; }
        public IEnumerable<ExpressionInfo> Conditions { get; set; }
    }
}
