using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Core.Attributes
{
    public class CacheableAttribute : Attribute
    {
        public string CacheKey { get; set; }

        public CacheItemPriority CachePriority { get; set; }

        public int CacheTimeOut { get; set; }

        public CacheableAttribute()
        {
            CachePriority = CacheItemPriority.Normal;
            CacheTimeOut = 20;
        }

        public CacheableAttribute(string cacheKey)
            : this(cacheKey, CacheItemPriority.Normal, 20)
        {
        }

        public CacheableAttribute(string cacheKey, CacheItemPriority priority)
            : this(cacheKey, priority, 20)
        {
        }

        public CacheableAttribute(string cacheKey, CacheItemPriority priority, int timeOut)
        {
            CacheKey = cacheKey;
            CachePriority = priority;
            CacheTimeOut = timeOut;
        }
    }
}
