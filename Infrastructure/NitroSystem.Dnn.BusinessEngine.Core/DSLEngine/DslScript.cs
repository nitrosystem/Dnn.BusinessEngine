using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Base;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine
{
    public sealed class DslScript
    {
        public string Version { get; set; }
        public IReadOnlyList<DslStatement> Statements { get; set; }
    }
}
