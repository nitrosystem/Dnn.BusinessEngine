using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models
{
    public sealed class DiagnosticEntry : IEntity
    {
        public Guid Id { get; set; }
        public DiagnosticScope Scope { get; set; } // Studio | Runtime
        public DiagnosticSeverity Severity { get; set; }

        public string Code { get; set; }           // BE-DSL-001
        public string Title { get; set; }
        public string Message { get; set; }

        public string Module { get; set; }          // WorkflowEngine, DslEngine, DAL
        public string Component { get; set; }       // ActionName, WorkflowName
        public string Operation { get; set; }       // Execute, Compile, Import

        public Guid TraceId { get; set; }         // VERY IMPORTANT
        public DateTime OccurredAt { get; set; }

        public DiagnosticContext Context { get; set; }
        public Exception Exception { get; set; }    // Optional
    }

}
