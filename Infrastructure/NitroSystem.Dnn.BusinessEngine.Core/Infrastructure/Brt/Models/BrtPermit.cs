using System;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.Infrastructure.Brt.Models
{
    // مدل مجوز ساده
    public class BrtPermit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Issuer { get; set; }             // چه لایه‌ای صادر کرده
        public string Purpose { get; set; }            // توضیح / نام مسیر
        public DateTimeOffset ExpiresAt { get; set; } // انقضاء
        public IDictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }
}
