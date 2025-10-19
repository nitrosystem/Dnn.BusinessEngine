using DotNetNuke.Services.Installer.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework
{
    public class LogContext
    {
        private readonly List<LogEntry> _entries = new();

        public Guid ScenarioId { get; private set; }
        public string ScenarioName { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public Exception LastError { get; private set; }

        public void StartScenario(string name, object meta = null)
        {
            ScenarioId = Guid.NewGuid();
            ScenarioName = name;
            StartedAt = DateTime.UtcNow;
        }

        public void CompleteScenario()
        {
            CompletedAt = DateTime.UtcNow;
        }

        public void FailScenario(Exception ex)
        {
            LastError = ex;
            CompletedAt = DateTime.UtcNow;
        }

        public async Task AddEntryAsync(LogEntry entry)
        {
            _entries.Add(entry);
            await Task.CompletedTask;
        }

        public IReadOnlyList<LogEntry> GetEntries() => _entries.AsReadOnly();
    }
}
