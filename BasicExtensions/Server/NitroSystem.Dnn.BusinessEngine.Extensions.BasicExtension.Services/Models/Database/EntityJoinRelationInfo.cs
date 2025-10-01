namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database
{
    public class EntityJoinRelationInfo
    {
        public string JoinType { get; set; }
        public string LeftEntityAliasName { get; set; }
        public string LeftEntityTableName { get; set; }
        public string RightEntityAliasName { get; set; }
        public string RightEntityTableName { get; set; }
        public string JoinConditions { get; set; }
    }
}