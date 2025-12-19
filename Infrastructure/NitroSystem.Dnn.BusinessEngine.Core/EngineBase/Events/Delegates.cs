using System;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase.Events
{
    public delegate Task EngineProgressHandler(string message, double? percent);
    public delegate Task EngineErrorHandler(Exception ex, IEngineContext context);
}
