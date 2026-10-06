using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Tables.Action
{
    [Table("BusinessEngine_ActionTypes")]
    [Cacheable("BE_ActionTypes_", CacheItemPriority.Default, 20)]
    public class ActionTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public string ActionDomain { get; set; }
        public string ActionType { get; set; }
        public string Title { get; set; }
        public int OperationType { get; set; }
        public string PageUrl { get; set; }
        public string ActionComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public string BusinessControllerClass { get; set; }
        public bool HideModuleBuilder { get; set; }
        public bool IsResultable { get; set; }
        public bool IsRedirectable { get; set; }
        public string DslSuggestions { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
