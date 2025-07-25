using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using NitroSystem.Dnn.BusinessEngine.Core.Attributes;
using NitroSystem.Dnn.BusinessEngine.Core.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.Attributes;

namespace NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables
{
    [Table("BusinessEngine_PaymentMethods")]
    [Cacheable("BE_PaymentMethods_", CacheItemPriority.Default, 20)]
    [Scope("ScenarioId")]
    public class PaymentMethodInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid ScenarioId { get; set; }
        public string PaymentMethodName { get; set; }
        public string SuccessfulPaymentTemplate{ get; set; }
        public string UnsuccessfulPaymentTemplate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public int LastModifiedByUserId { get; set; }
        public int ViewOrder { get; set; }
    }
}