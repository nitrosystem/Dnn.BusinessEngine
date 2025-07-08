using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Studio.Data.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contract;

namespace NitroSystem.Dnn.BusinessEngine.Studio.Data.Entities.Views
{
    [Table("BusinessEngineView_Libraries")]
    [Cacheable("BusinessEngine_Libraries_View_", CacheItemPriority.Default, 20)]
    public class LibraryView : IEntity
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string LibraryName { get; set; }
        public string LibraryLogo { get; set; }
        public string Version { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public int LoadOrder { get; set; }
    }
}