using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base
{
    public abstract class DslExpression : DslNode
    {
        public Type ResolvedType { get; set; } // بعداً در Validation پر می‌شود
    }
}
