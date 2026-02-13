using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DiagnosticCenter
{
    public sealed class DiagnosticEntryBuilder
    {
        private readonly DiagnosticEntry _entry;
        private readonly DiagnosticContext _context;

        private DiagnosticEntryBuilder(DiagnosticScope scope)
        {
            _context = new DiagnosticContext();

            _entry = new DiagnosticEntry
            {
                Id = Guid.NewGuid(),
                Scope = scope,
                OccurredAt = DateTime.UtcNow,
                Context = _context
            };
        }

        /* ---------- Entry points ---------- */

        public static DiagnosticEntryBuilder Runtime()
            => new DiagnosticEntryBuilder(DiagnosticScope.Runtime);

        public static DiagnosticEntryBuilder Studio()
            => new DiagnosticEntryBuilder(DiagnosticScope.Studio);

        /* ---------- Severity ---------- */

        public DiagnosticEntryBuilder Trace(string code, string title)
            => SetSeverity(DiagnosticSeverity.Trace, code, title);

        public DiagnosticEntryBuilder Info(string code, string title)
            => SetSeverity(DiagnosticSeverity.Info, code, title);

        public DiagnosticEntryBuilder Warning(string code, string title)
            => SetSeverity(DiagnosticSeverity.Warning, code, title);

        public DiagnosticEntryBuilder Error(string code, string title)
            => SetSeverity(DiagnosticSeverity.Error, code, title);

        public DiagnosticEntryBuilder Critical(string code, string title)
            => SetSeverity(DiagnosticSeverity.Critical, code, title);

        private DiagnosticEntryBuilder SetSeverity(
            DiagnosticSeverity severity,
            string code,
            string title)
        {
            _entry.Severity = severity;
            _entry.Code = code;
            _entry.Title = title;
            return this;
        }

        /* ---------- Description ---------- */

        public DiagnosticEntryBuilder WithMessage(string message)
        {
            _entry.Message = message;
            return this;
        }

        /* ---------- Source ---------- */

        public DiagnosticEntryBuilder From(
            string module,
            string component,
            string operation)
        {
            _entry.Module = module;
            _entry.Component = component;
            _entry.Operation = operation;
            return this;
        }

        /* ---------- Trace ---------- */

        public DiagnosticEntryBuilder WithTraceId(Guid traceId)
        {
            _entry.TraceId = traceId;
            return this;
        }

        /* ---------- Context ---------- */

        public DiagnosticEntryBuilder WithContext(
            Action<DiagnosticContext> configure)
        {
            configure?.Invoke(_context);
            return this;
        }

        public DiagnosticEntryBuilder AddData(string key, object value)
        {
            _context.Data[key] = value;
            return this;
        }

        /* ---------- Exception ---------- */

        public DiagnosticEntryBuilder WithException(Exception exception)
        {
            _entry.Exception = exception;
            return this;
        }

        /* ---------- Finalize ---------- */

        public DiagnosticEntry Build()
        {
            Validate();
            return _entry;
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(_entry.Code))
                throw new InvalidOperationException("Diagnostic Code is required.");

            if (string.IsNullOrWhiteSpace(_entry.Title))
                throw new InvalidOperationException("Diagnostic Title is required.");

            if (string.IsNullOrWhiteSpace(_entry.Module))
                throw new InvalidOperationException("Diagnostic Module is required.");
        }
    }
}
