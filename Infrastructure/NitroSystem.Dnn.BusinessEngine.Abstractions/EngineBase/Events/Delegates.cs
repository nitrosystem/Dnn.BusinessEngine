using System;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.EngineBase.Events
{
    // دلیگیت‌ها / ایونت‌ها
    public delegate Task EngineProgressHandler(string message, double? percent);
    public delegate Task EngineErrorHandler(Exception ex, string phase);
    public delegate Task EngineSuccessHandler<TResponse>(TResponse response);
}
