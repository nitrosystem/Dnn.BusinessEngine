using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Models
{
    public sealed class SmsMessage
    {
        public string To { get; set; }
        public string Body { get; set; }
        public string From { get; set; }
        public string TemplateCode { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public SmsPriority Priority { get; set; } = SmsPriority.Normal;
    }

}
