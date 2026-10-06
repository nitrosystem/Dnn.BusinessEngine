using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base
{
    public abstract class ExpressionBase
    {
        public virtual string ExpressionPath { get; set; }
        public Type ResolvedType { get; set; } //It will be filled in the Validation.
    }
}
