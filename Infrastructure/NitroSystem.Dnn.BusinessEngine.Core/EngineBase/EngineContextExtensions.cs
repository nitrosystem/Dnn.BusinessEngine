
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.EngineBase;

namespace NitroSystem.Dnn.BusinessEngine.Core.EngineBase
{
    public static class EngineContextExtensions
    {
        public static void Set<T>(this EngineContext context, string key, T value)
            => context.Items[key] = value;

        public static T Get<T>(this EngineContext context, string key)
            => context.Items.TryGetValue(key, out var value) ? (T)value : default;
    }

}
