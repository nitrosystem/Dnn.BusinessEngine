using NitroSystem.Dnn.BusinessEngine.Core.Providers.SMS.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
