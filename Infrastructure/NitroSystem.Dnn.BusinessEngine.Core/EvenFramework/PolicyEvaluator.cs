using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class PolicyEvaluator
    {
        private readonly IMetricCollector _metrics;

        public PolicyEvaluator(IMetricCollector metrics)
        {
            _metrics = metrics;
        }

        public PolicyEvaluationResult Evaluate(ThresholdRule rule, EventContext ctx, string nodeName = null)
        {
            if (!rule.Enabled) return new PolicyEvaluationResult { Rule = rule, Triggered = false };

            // cooldown check
            if (rule.Cooldown.HasValue && rule.LastTriggered.HasValue)
            {
                if (DateTime.UtcNow - rule.LastTriggered.Value < rule.Cooldown.Value)
                    return new PolicyEvaluationResult { Rule = rule, Triggered = false };
            }

            // pick metric
            double? metricVal = null;
            if (rule.Scope == RuleScope.Node && !string.IsNullOrEmpty(rule.TargetNodeName))
                metricVal = _metrics.GetMetric(ctx.Id, rule.TargetNodeName, rule.Metric);
            else // event/global
                metricVal = _metrics.GetMetric(ctx.Id, null, rule.Metric) ?? _metrics.GetMetric(ctx.Id, "", rule.Metric);

            if (!metricVal.HasValue) return new PolicyEvaluationResult { Rule = rule, Triggered = false, MetricValue = null };

            bool triggered = Compare(metricVal.Value, rule.Operator, rule.Value);
            return new PolicyEvaluationResult { Rule = rule, Triggered = triggered, MetricValue = metricVal.Value };
        }

        private bool Compare(double left, string op, double right)
        {
            return op switch
            {
                ">" => left > right,
                ">=" => left >= right,
                "<" => left < right,
                "<=" => left <= right,
                "==" => Math.Abs(left - right) < 1e-9,
                "!=" => Math.Abs(left - right) >= 1e-9,
                _ => false
            };
        }
    }
}
