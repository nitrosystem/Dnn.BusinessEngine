using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    /// <summary>
    /// متدهای کمکی برای materialize هوشمند مجموعه‌ها با درک از context و cache
    /// </summary>
    public static class SmartMaterializer
    {
        /// <summary>
        /// داده را بر اساس شرایط مختلف (Context, Cache, Reuse) بهینه materialize می‌کند.
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="source">مجموعه ورودی</param>
        /// <param name="context">کانتکست فعلی Engine</param>
        /// <param name="cacheService">سرویس کش (اختیاری)</param>
        /// <param name="cacheKey">کلید کش برای ذخیره‌سازی نتایج</param>
        /// <param name="forceMaterialize">در صورت true همیشه materialize می‌کند</param>
        /// <param name="expectedReuses">تخمین تعداد دفعات استفاده مجدد از داده</param>
        /// <returns>لیست نهایی (materialized یا cached)</returns>
        public static async Task<IEnumerable<T>> MaterializeSmartAsync<T>(
            this IEnumerable<T> source,
            EngineContext context = null,
            ICacheService cacheService = null,
            string cacheKey = null,
            bool forceMaterialize = false,
            int expectedReuses = 1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // اگر از قبل materialized است
            if (source is ICollection<T> || source is T[])
                return source;

            // اگر Cache فعال است و کلید داده شده
            if (cacheService != null && !string.IsNullOrEmpty(cacheKey))
            {
                var cached = cacheService.Get<IEnumerable<T>>(cacheKey);
                if (cached != null)
                    return cached;
            }

            // بررسی شرایط context برای تشخیص نیاز به materialization
            bool shouldMaterialize = forceMaterialize || expectedReuses > 1;

            var result = shouldMaterialize ? source.ToList() : source;

            // اگر باید cache شود
            if (cacheService != null && !string.IsNullOrEmpty(cacheKey))
            {
                cacheService.Set(cacheKey, result);
            }

            return result;
        }

        /// <summary>
        /// نسخه synchronous از MaterializeSmart که Context و Cache-aware است.
        /// </summary>
        public static IEnumerable<T> MaterializeSmart<T>(
            this IEnumerable<T> source,
            EngineContext context = null,
            ICacheService cacheService = null,
            string cacheKey = null,
            bool forceMaterialize = false,
            int expectedReuses = 1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // اگر منبع از قبل materialized است
            if (source is ICollection<T> || source is T[])
                return source;

            // بررسی کش (در حالت sync)
            if (cacheService != null && !string.IsNullOrEmpty(cacheKey))
            {
                var cached = cacheService.Get<IEnumerable<T>>(cacheKey);
                if (cached != null)
                    return cached;
            }

            // تصمیم‌گیری هوشمندانه
            bool shouldMaterialize = forceMaterialize || expectedReuses > 1;
            var result = shouldMaterialize ? source.ToList() : source;

            // ثبت در کش در صورت نیاز
            if (cacheService != null && !string.IsNullOrEmpty(cacheKey))
            {
                cacheService.Set(cacheKey, result);
            }

            return result;
        }

        public static async Task<IEnumerable<T>> MaterializeSmartAsync<T>(
        this Task<IEnumerable<T>> sourceTask,
        EngineContext context = null,
        ICacheService cacheService = null,
        string cacheKey = null,
        bool forceMaterialize = false,
        int expectedReuses = 1)
        {
            var source = await sourceTask.ConfigureAwait(false);
            return await source.MaterializeSmartAsync(
                context,
                cacheService,
                cacheKey,
                forceMaterialize,
                expectedReuses
            ).ConfigureAwait(false);
        }
    }
}
