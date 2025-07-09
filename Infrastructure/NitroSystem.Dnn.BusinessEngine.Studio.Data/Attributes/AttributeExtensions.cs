
using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Cashing;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes
{
    public static class AttributeExtensions
    {
        public static string GetTableName<T>(this AttributeCache attributeCache) where T : class, IEntity, new()
        {
            var item = attributeCache.GetAttribute<T, TableAttribute>();
            return item.TableName;
        }

        public static string GetScope<T>(this AttributeCache attributeCache) where T : class, IEntity, new()
        {
            var item = attributeCache.GetAttribute<T, ScopeAttribute>();
            return item.Scope;
        }

        public static (string key, int? timeOut) GetCache<T>(this AttributeCache attributeCache) where T : class, IEntity, new()
        {
            var item = attributeCache.GetAttribute<T, CacheableAttribute>();
            return item != null ? (item.CacheKey, item.CacheTimeOut) : (string.Empty, 0);
        }

        public static IEntity GetEntity<T>(this AttributeCache attributeCache) where T : class
        {
            var item = attributeCache.GetAttribute<T, EntityAttribute>();
            return item.Entity;
        }
    }
}
