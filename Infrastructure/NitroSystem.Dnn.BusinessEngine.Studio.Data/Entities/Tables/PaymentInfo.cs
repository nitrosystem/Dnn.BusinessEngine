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
    [Table("BusinessEngine_Payments")]
    [Cacheable("BE_Payments_", CacheItemPriority.Default, 20)]
    [Scope("PaymentMethodID")]
    public class PaymentInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid PaymentMethodId { get; set; }
        public Guid ModuleId { get; set; }
        public int UserId { get; set; }
        public string PaymentKey { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public DateTime ResponseDateTime { get; set; }
        public bool IsSuccess { get; set; }
        public int Status { get; set; }
        public string ReferenceNumber { get; set; }
        public string PaymentGateway { get; set; }
        public string PaymentParams { get; set; }
        public string ErrorMessage { get; set; }
    }
}