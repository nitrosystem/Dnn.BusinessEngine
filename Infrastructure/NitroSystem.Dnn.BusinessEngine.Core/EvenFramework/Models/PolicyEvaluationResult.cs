using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models
{
    public class PolicyEvaluationResult
    {
        public ThresholdRule Rule { get; set; }
        public bool Triggered { get; set; }
        public double? MetricValue { get; set; }
    }
}
