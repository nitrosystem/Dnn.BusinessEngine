using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ExpressionFunctionAttribute : Attribute
    {
        public string Name { get; }
        public ExpressionFunctionAttribute(string name) => Name = name;
    }
}
