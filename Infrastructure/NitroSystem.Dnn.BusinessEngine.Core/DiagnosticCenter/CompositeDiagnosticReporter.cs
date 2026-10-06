using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter
{
    public sealed class CompositeDiagnosticReporter : IDiagnosticReporter
    {
        private readonly IEnumerable<IDiagnosticReporter> _reporters;

        public CompositeDiagnosticReporter(IEnumerable<IDiagnosticReporter> reporters)
        {
            _reporters = reporters;
        }

        public void Report(DiagnosticEntry entry)
        {
            foreach (var reporter in _reporters)
                reporter.Report(entry);
        }
    }
}
