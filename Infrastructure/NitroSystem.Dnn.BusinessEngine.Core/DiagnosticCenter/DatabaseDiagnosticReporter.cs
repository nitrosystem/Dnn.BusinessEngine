using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
