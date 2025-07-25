
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_ActionConditions")]
    [Cacheable("BE_ActionConditions_", CacheItemPriority.Default, 20)]
    [Scope("ActionId")]
    public class ActionConditionInfo : IEntity, IExpression
    {
        public Guid Id { get; set; }
        public Guid ActionId { get; set; }
        public string LeftExpression { get; set; }
        public string EvalType { get; set; }
        public string RightExpression { get; set; }
        public string GroupName { get; set; }
        public string ExpressionParsingType { get; set; }
        public int ViewOrder { get; set; }
    }
}
