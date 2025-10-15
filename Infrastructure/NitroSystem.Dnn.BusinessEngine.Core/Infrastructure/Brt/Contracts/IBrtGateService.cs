using System;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts
{
    // نمای کلی گِیت BRT
    public interface IBrtGateService
    {
        // ایجاد/ثبت مجوز (ممکن است توسط سرویس مجوزدهنده فراخوانی شود)
        Task RegisterPermitAsync(BrtPermit permit);

        // اعتبارسنجی (یا رد) یک مجوز
        Task<bool> ValidatePermitAsync(Guid permitId);

        // باز کردن مسیر با مجوز — برمی‌گرداند IDisposable که با Dispose مسیر را می‌بندد
        Task<IDisposable> OpenGateAsync(Guid permitId, CancellationToken ct = default);

        // وضعیت مسیر (مثلاً برای مانیتورینگ)
        Task<bool> IsGateOpenAsync(Guid permitId);
    }
}
