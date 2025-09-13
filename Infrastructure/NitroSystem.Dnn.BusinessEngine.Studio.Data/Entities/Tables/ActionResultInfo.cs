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
    [Table("BusinessEngine_ActionResults")]
    [Cacheable("BE_ActionResults_", CacheItemPriority.Default, 20)]
    [Scope("ActionId")]
    public class ActionResultInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ActionId { get; set; }
        public string LeftExpression { get; set; }
        public string EvalType { get; set; }
        public string RightExpression { get; set; }
        public string Conditions { get; set; }
        public int ViewOrder { get; set; }
    }
}
