using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ScopeAttribute :  Attribute
    {
        public string Scope { get; }

        public ScopeAttribute(string scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }
    }
}
