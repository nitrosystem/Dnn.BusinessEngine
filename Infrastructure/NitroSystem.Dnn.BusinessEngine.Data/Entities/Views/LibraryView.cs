using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_Libraries")]
    [Cacheable("BE_Libraries_View_", CacheItemPriority.Default, 20)]
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