using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine
{
    public sealed class DslScript
    {
        public string Version { get; set; }
        public IReadOnlyList<DslStatement> Statements { get; set; }
    }
}
