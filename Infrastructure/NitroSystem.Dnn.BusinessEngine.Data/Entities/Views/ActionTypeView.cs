using System;
using System.Web.Caching;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Shared.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Views
{
    [Table("BusinessEngineView_ActionTypes")]
    [Cacheable("BE_ActionTypes_View_", CacheItemPriority.Default, 20)]
    public class ActionTypeView : IEntity
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public string ActionType { get; set; }
        public string Title { get; set; }
        public string ActionComponent { get; set; }
        public string ComponentSubParams { get; set; }
        public int ExecutionScope { get; set; }
        public bool HasResults { get; set; }
        //public int CacheState { get; set; }
        public string BusinessControllerClass { get; set; }
        public string ActionJsPath { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int GroupViewOrder { get; set; }
        public int ViewOrder { get; set; }
    }
}