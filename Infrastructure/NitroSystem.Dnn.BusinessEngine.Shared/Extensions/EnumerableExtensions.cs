using System;
using System.Collections.Generic;
using System.Linq;

namespace NitroSystem.Dnn.BusinessEngine.Shared.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// اگر منبع داده از قبل materialized نشده باشد، بر اساس نیاز آن را materialize می‌کند.
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="source">داده ورودی</param>
        /// <param name="forceMaterialize">در صورت true همیشه materialize می‌کند</param>
        /// <param name="expectedReuses">تخمین تعداد دفعاتی که داده استفاده می‌شود</param>
        /// <returns>IEnumerable<T> که در صورت نیاز materialized شده است</returns>
        public static IEnumerable<T> MaterializeIfNeeded<T>(
            this IEnumerable<T> source,
            bool forceMaterialize = false,
            int expectedReuses = 1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // اگر منبع از قبل لیست یا آرایه است، نیازی به کار خاصی نیست
            if (source is ICollection<T> || source is T[])
                return source;

            // اگر مشخص شده فقط یکبار استفاده می‌شود و اجباری هم نیست، پس نیازی نیست
            if (!forceMaterialize && expectedReuses <= 1)
                return source;

            // در غیر اینصورت، Materialize می‌کنیم
            return source.ToList();
        }
    }
}
