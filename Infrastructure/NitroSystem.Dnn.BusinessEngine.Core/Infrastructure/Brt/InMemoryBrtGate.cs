using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt
{
    public class InMemoryBrtGate : IBrtGateService
    {
        // نگهداری پرمیت‌ها
        private readonly ConcurrentDictionary<Guid, BrtPermit> _permits = new();
        // نگهداری گِیت‌های باز -> شمارنده تا چند بار باز شده توسط هم‌زمانی‌های مختلف
        private readonly ConcurrentDictionary<Guid, int> _openCounts = new();

        // کانتکست محلی برای علامت‌گذاری داخل BRT
        private static readonly AsyncLocal<Stack<Guid>?> _current = new();

        public Task RegisterPermitAsync(BrtPermit permit)
        {
            _permits[permit.Id] = permit;
            return Task.CompletedTask;
        }

        public Task<bool> ValidatePermitAsync(Guid permitId)
        {
            if (!_permits.TryGetValue(permitId, out var p)) return Task.FromResult(false);
            if (p.ExpiresAt < DateTimeOffset.UtcNow) return Task.FromResult(false);
            // جا برای سیاست‌های بیشتر (اجازه‌ها، گزینه‌ها و ...)
            return Task.FromResult(true);
        }

        public async Task<IDisposable> OpenGateAsync(Guid permitId, CancellationToken ct = default)
        {
            if (!await ValidatePermitAsync(permitId)) throw new UnauthorizedAccessException("Invalid or expired permit");

            _openCounts.AddOrUpdate(permitId, 1, (_, v) => v + 1);

            // push to AsyncLocal
            var stack = _current.Value ??= new Stack<Guid>();
            stack.Push(permitId);

            return Disposable.Create(() =>
            {
                var s = _current.Value ?? new Stack<Guid>(); // همیشه stack معتبر
                if (s.Count > 0) s.Pop();

                _openCounts.AddOrUpdate(permitId, 0, (_, v) => Math.Max(0, v - 1));
                if (_openCounts.TryGetValue(permitId, out var cnt) && cnt == 0)
                {
                    _openCounts.TryRemove(permitId, out _);
                    // اینجا می‌توان لاگ یا اتفاقات فورمال دیگری انجام داد
                }
            });
        }

        public Task<bool> IsGateOpenAsync(Guid permitId)
        {
            return Task.FromResult(_openCounts.ContainsKey(permitId));
        }

        // دسترسی به کانتکست فعلی (برای چک در داخل کارخانه‌ها)
        public static bool IsInGateContext(Guid permitId)
        {
            var s = _current.Value;
            return s != null && s.Count > 0 && s.Peek() == permitId;
        }
    }
}
