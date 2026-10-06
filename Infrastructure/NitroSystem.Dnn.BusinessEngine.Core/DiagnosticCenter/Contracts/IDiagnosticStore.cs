using System.Threading.Tasks;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts
{
    public interface IDiagnosticStore
    {
        IReadOnlyList<DiagnosticEntry> Query(DiagnosticQuery query);
        Task Save(DiagnosticEntry entry);
    }
}
