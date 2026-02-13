using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts
{
    public interface IDiagnosticStore
    {
        Task Save(DiagnosticEntry entry);
        //IReadOnlyList<DiagnosticEntry> Query(DiagnosticQuery query);
    }
}
