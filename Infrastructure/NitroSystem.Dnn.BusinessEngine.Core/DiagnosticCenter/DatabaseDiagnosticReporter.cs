using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter
{
    public sealed class DatabaseDiagnosticReporter : IDiagnosticReporter
    {
        private readonly IDiagnosticStore _store;

        public DatabaseDiagnosticReporter(IDiagnosticStore store)
        {
            _store = store;
        }

        public void Report(DiagnosticEntry entry)
        {
            _store.Save(entry);
        }
    }
}
