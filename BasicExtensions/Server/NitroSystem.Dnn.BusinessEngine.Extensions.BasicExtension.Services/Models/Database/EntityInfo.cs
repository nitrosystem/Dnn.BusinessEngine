using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database
{
    public class EntityInfo
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; }
        public string AliasName { get; set; }
        public string TableName { get; set; }
        public bool EnableJoin { get; set; }
        public IEnumerable<EntityJoinRelationInfo> JoinRelationships { get; set; }
    }
}