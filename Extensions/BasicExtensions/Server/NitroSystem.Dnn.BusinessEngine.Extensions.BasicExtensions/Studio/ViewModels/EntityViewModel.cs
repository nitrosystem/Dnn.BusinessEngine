using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Studio.ViewModels
{
    public class EntityViewModel
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string AliasName { get; set; }
        public string TableName { get; set; }
        public bool EnableJoin { get; set; }
        public IEnumerable<EntityJoinRelationViewModel> JoinRelationships { get; set; }
    }
}