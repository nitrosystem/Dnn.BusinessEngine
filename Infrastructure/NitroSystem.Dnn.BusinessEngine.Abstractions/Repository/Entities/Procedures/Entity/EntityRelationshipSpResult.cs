using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Repository.Entities.Procedures.Entity
{
    public class EntityRelationshipSpResult
    {
        public Guid EntityId { get; set; }
        public string RelationshipName { get; set; }
        public string ParentTableName { get; set; }
        public bool EnforceForReplication { get; set; }
        public bool EnforceForeignKeyConstraint { get; set; }
        public string DeleteSpecification { get; set; }
        public string UpdateSpecification { get; set; }
        public string PrimaryKeys { get; set; }
        public string ForeignKeys { get; set; }
    }
}
