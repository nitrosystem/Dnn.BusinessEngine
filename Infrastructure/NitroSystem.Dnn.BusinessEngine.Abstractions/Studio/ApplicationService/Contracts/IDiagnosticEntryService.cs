using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.DiagnosticEntry;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.Contracts
{
    public interface IDiagnosticEntryService
    {
        Task SaveDiagnosticEntryAsync(DiagnosticEntryViewModel diagnosticEntry);
    }
}
