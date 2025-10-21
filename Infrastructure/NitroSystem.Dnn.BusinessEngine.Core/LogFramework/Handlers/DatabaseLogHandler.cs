using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.LogFramework.Handlers
{
    public class DatabaseLogHandler : ILogHandler
    {
        public async Task WriteAsync(LogEntry entry)
        {
            // اینجا صرفاً نمایشی است:
            await Task.Run(() =>
            {
                // insert into Logs (Level, Message, Timestamp, User, Source) values ...
            });
        }
    }
}
