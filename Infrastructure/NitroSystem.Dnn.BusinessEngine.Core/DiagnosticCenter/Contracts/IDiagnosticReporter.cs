using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts
{
    public interface IDiagnosticReporter
    {
        void Report(DiagnosticEntry entry);
    }
}
