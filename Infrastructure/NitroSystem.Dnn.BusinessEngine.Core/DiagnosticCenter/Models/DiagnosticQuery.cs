using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models
{
    public sealed class DiagnosticQuery
    {
        /* ---------- Scope & Severity ---------- */

        public DiagnosticScope? Scope { get; set; }
        public IReadOnlyCollection<DiagnosticSeverity> Severities { get; set; }

        /* ---------- Identity & Correlation ---------- */

        public string TraceId { get; set; }

        public string ApplicationId { get; set; }
        public string WorkflowId { get; set; }
        public string ActionId { get; set; }

        /* ---------- Source ---------- */

        public string Module { get; set; }
        public string Component { get; set; }
        public string Operation { get; set; }

        /* ---------- Time ---------- */

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        /* ---------- Text Search ---------- */

        public string Code { get; set; }
        public string Keyword { get; set; } // Message / Title

        /* ---------- Paging ---------- */

        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 50;

        /* ---------- Sorting ---------- */

        public DiagnosticSort Sort { get; set; } = DiagnosticSort.Newest;
    }
}
