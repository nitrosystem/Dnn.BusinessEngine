using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts
{
    public interface IDiagnosticReporter
    {
        void Report(DiagnosticEntry entry);
    }
}
