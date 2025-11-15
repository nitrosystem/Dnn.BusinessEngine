using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models
{
    public class ThresholdRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; }
        public RuleScope Scope { get; set; } = RuleScope.Node;
        public string TargetNodeName { get; set; } // اگر Scope==Node
        public string Metric { get; set; } // "nodeDuration","errorCount","custom:dbCalls" ...
        public string Operator { get; set; } // ">",">=","<","==","!="
        public double Value { get; set; } // مقدار مقایسه
        public RuleAction Action { get; set; } = RuleAction.Cancel;
        public int RetryCount { get; set; } = 0; // برای Retry
        public TimeSpan? Cooldown { get; set; } // فاصله زمانی پس از اجرای Action
        public DateTime? LastTriggered { get; set; } // برای اعمال cooldown
        public bool Enabled { get; set; } = true;

        // Optional: callback names or other metadata to hook into rollback
        public string RollbackCallbackKey { get; set; }
    }
}
