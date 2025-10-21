using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Core.LogFramework.Contracts
{
    public interface ILogHandler
    {
        Task HandleAsync(LogEntry entry);
    }
}
