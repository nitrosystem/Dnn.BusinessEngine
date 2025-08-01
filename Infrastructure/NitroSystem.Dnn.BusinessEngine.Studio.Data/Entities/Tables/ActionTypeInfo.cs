
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ActionTypes")]
    [Cacheable("BE_ActionTypes_", CacheItemPriority.Default, 20)]
    public class ActionTypeInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ExtensionId { get; set; }
        public Guid GroupId { get; set; }
        public string ActionType { get; set; }
        public string Title { get; set; }
        public string ActionComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public int ExecutionScope { get; set; }
        public bool HasResults { get; set; }
        public string BusinessControllerClass { get; set; }
        public string ActionJsPath { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
    }
}
