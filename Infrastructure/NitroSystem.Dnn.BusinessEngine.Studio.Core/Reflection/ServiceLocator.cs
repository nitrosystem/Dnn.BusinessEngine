using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.Reflection
{
    public class ServiceLocator : IServiceLocator
    {
        // Thread-safe type cache
        private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

        public T CreateInstance<T>(string typeName, params object[] parameters) where T : class
        {
            // تلاش برای دریافت نوع از کش، در غیر این صورت اضافه کن
            var type = _typeCache.GetOrAdd(typeName, key =>
            {
                var resolvedType = Type.GetType(key);

                if (resolvedType == null)
                    throw new TypeLoadException($"Type not found: {key}");

                return resolvedType;
            });

            // بررسی انتساب‌پذیری به نوع T
            if (!typeof(T).IsAssignableFrom(type))
                throw new InvalidCastException($"Type {typeName} is not assignable to {typeof(T).FullName}");

            // ایجاد نمونه جدید (هر بار instance تازه ولی نوع cached شده)
            return Activator.CreateInstance(type, parameters) as T
                   ?? throw new Exception($"Instance creation failed for type: {type.FullName}");
        }
    }
}
