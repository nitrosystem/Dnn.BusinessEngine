using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Enums;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework
{
    public class LogLevelFilterMiddleware : ILogMiddleware
    {
        private readonly LogLevel _minLevel;

        public LogLevelFilterMiddleware(LogLevel minLevel)
        {
            _minLevel = minLevel;
        }

        public async Task InvokeAsync(LogEntry entry, Func<Task> next)
        {
            if (entry.Level >= _minLevel)
                await next();
        }
    }
}
