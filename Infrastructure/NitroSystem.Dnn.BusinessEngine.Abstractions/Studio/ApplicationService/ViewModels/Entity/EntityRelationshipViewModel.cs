using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.ApplicationService.ViewModels.Entity
{
    public class EntityRelationshipViewModel
    {
        public string RelationshipName { get; set; }
        public string ParentTableName { get; set; }
        public bool EnforceForReplication { get; set; }
        public bool EnforceForeignKeyConstraint { get; set; }
        public string DeleteSpecification { get; set; }
        public string UpdateSpecification { get; set; }
        public IEnumerable<string> PrimaryKeys { get; set; }
        public IEnumerable<string> ForeignKeys { get; set; }
    }
}
