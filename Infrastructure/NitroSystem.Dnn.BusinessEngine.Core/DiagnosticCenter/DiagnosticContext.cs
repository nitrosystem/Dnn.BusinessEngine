using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter
{
    public sealed class DiagnosticContext
    {
        public Guid? ScenarioId { get; set; }
        public Guid? ModuleId { get; set; }
        public Guid EntryId { get; set; }
        public int UserId { get; set; }

        public IDictionary<string, object> Data { get; set; } =
            new Dictionary<string, object>();
    }
}
