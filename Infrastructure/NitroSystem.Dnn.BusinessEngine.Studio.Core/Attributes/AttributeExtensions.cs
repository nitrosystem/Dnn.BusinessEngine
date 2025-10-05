using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    public static class AttributeExtensions
    {
        public static string GetTableName<T>(this AttributeCache attributeCache) where T : class, IEntity, new()
        {
            var item = attributeCache.GetAttribute<T, TableAttribute>();
            return item.TableName;
        }

        public static (string key, int? timeOut) GetCache<T>(this AttributeCache attributeCache) where T : class, IEntity, new()
        {
            var item = attributeCache.GetAttribute<T, CacheableAttribute>();
            return item != null ? (item.CacheKey, item.CacheTimeOut) : (string.Empty, 0);
        }

        public static string GetScope<T>(this AttributeCache attributeCache) where T : class, IEntity, new()
        {
            var item = attributeCache.GetAttribute<T, ScopeAttribute>();
            return item.Scope;
        }
    }
}
