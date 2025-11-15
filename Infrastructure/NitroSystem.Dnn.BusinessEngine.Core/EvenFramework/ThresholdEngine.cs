using Microsoft.Extensions.Logging;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Contracts;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.EvenFramework.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.EvenFramework
{
    public class ThresholdEngine : IEventObserver
    {
        private readonly IList<ThresholdRule> _rules = new List<ThresholdRule>();
        private readonly IMetricCollector _metrics;
        private readonly PolicyEvaluator _evaluator;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<Guid, int> _retryCounts = new();

        // callback hooks (optional) برای rollback یا custom actions
        private readonly ConcurrentDictionary<string, Func<EventContext, Task>> _rollbackCallbacks = new();

        public ThresholdEngine(IMetricCollector metrics, ILogger logger = null)
        {
            _metrics = metrics;
            _evaluator = new PolicyEvaluator(metrics);
            _logger = logger;
        }

        public void AddRule(ThresholdRule rule) => _rules.Add(rule);
        public void RegisterRollbackCallback(string key, Func<EventContext, Task> callback) => _rollbackCallbacks[key] = callback;

        public Task OnStartAsync(EventContext context)
        {
            // nothing
            return Task.CompletedTask;
        }

        public Task OnNodeCompleteAsync(EventContext context, string nodeName, TimeSpan duration)
        {
            // record nodeDuration
            _metrics.RecordNodeMetric(context.Id, nodeName, "nodeDuration", duration.TotalMilliseconds);

            // evaluate rules relevant for node
            foreach (var rule in _rules.Where(r => r.Enabled && (r.Scope == RuleScope.Node ? r.TargetNodeName == nodeName : (r.Scope == RuleScope.Event || r.Scope == RuleScope.Global))))
            {
                var result = _evaluator.Evaluate(rule, context, nodeName);
                if (result.Triggered)
                {
                    HandleTriggeredRuleAsync(rule, context).Forget(); // fire-and-forget safest: but we'll await small actions
                }
            }

            return Task.CompletedTask;
        }

        public Task OnSuccessAsync(EventContext context)
        {
            // clear metrics for event if desired
            _metrics.ClearForEvent(context.Id);
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(EventContext context, Exception ex)
        {
            // increment errorCount metric
            var prev = _metrics.GetMetric(context.Id, "", "errorCount") ?? 0;
            _metrics.RecordNodeMetric(context.Id, "", "errorCount", prev + 1);

            // evaluate event-level rules
            foreach (var rule in _rules.Where(r => r.Enabled && (r.Scope == RuleScope.Event || r.Scope == RuleScope.Global)))
            {
                var result = _evaluator.Evaluate(rule, context, null);
                if (result.Triggered)
                {
                    HandleTriggeredRuleAsync(rule, context).Forget();
                }
            }

            return Task.CompletedTask;
        }

        public Task OnCancelledAsync(EventContext context)
        {
            _logger?.LogInformation($"Event {context.EventName} cancelled - ThresholdEngine recorded");
            _metrics.ClearForEvent(context.Id);
            return Task.CompletedTask;
        }

        private async Task HandleTriggeredRuleAsync(ThresholdRule rule, EventContext ctx)
        {
            rule.LastTriggered = DateTime.UtcNow;
            _logger?.LogWarning($"Rule {rule.Name} triggered for event {ctx.EventName} - action: {rule.Action}");

            switch (rule.Action)
            {
                case RuleAction.Warn:
                    ctx.AddNode($"Rule {rule.Name} triggered");
                    //ctx.AddNode("Warning", $"Rule {rule.Name} triggered", "ThresholdEngine");
                    break;

                case RuleAction.Cancel:
                    ctx.AddNode($"Rule {rule.Name} caused cancellation");
                    //ctx.AddNode("Critical", $"Rule {rule.Name} caused cancellation", "ThresholdEngine");
                    await CancelEvent(ctx);
                    break;

                case RuleAction.Retry:
                    await TryRetry(rule, ctx);
                    break;

                case RuleAction.Rollback:
                    await DoRollback(rule, ctx);
                    break;
            }
        }

        private Task CancelEvent(EventContext ctx)
        {
            // convention: EventContext.Items["__cts"] holds CancellationTokenSource
            if (ctx.Items.TryGetValue("__cts", out var obj) && obj is CancellationTokenSource cts)
            {
                cts.Cancel();
                ctx.AddNode("Cancelled by ThresholdEngine");
                //ctx.AddNode("Info", "Cancelled by ThresholdEngine", "ThresholdEngine");
            }
            else
            {
                ctx.AddNode("No CTS found to cancel");
                //ctx.AddNode("Warning", "No CTS found to cancel", "ThresholdEngine");
            }
            return Task.CompletedTask;
        }

        private async Task TryRetry(ThresholdRule rule, EventContext ctx)
        {
            var key = ctx.Id;
            var curr = _retryCounts.AddOrUpdate(key, 1, (_, old) => old + 1);

            if (curr <= rule.RetryCount)
            {
                ctx.AddNode($"Retrying ({curr}/{rule.RetryCount}) due to rule {rule.Name}");
                //ctx.AddNode("Info", $"Retrying ({curr}/{rule.RetryCount}) due to rule {rule.Name}", "ThresholdEngine");
                // simple strategy: cancel current and re-invoke externally (EventManager or caller must support re-run)
                await CancelEvent(ctx);
                // Optionally push a work item to BackgroundFramework to re-run after short delay
            }
            else
            {
                ctx.AddNode($"Retry limit reached for rule {rule.Name}");
                //ctx.AddNode("Error", $"Retry limit reached for rule {rule.Name}", "ThresholdEngine");
                await CancelEvent(ctx);
            }
        }

        private async Task DoRollback(ThresholdRule rule, EventContext ctx)
        {
            ctx.AddNode($"Invoking rollback for rule {rule.Name}");
            //ctx.AddLog("Info", $"Invoking rollback for rule {rule.Name}", "ThresholdEngine");
            if (!string.IsNullOrEmpty(rule.RollbackCallbackKey) && _rollbackCallbacks.TryGetValue(rule.RollbackCallbackKey, out var cb))
            {
                try { await cb(ctx); }
                catch (Exception ex)
                {
                    ctx.AddNode($"Rollback callback failed: {ex.Message}");
                    //ctx.AddLog("Error", $"Rollback callback failed: {ex.Message}", "ThresholdEngine", ex); 
                }
            }
            else
            {
                ctx.AddNode("No rollback callback registered");
                //ctx.AddLog("Warning", "No rollback callback registered", "ThresholdEngine");
            }
        }
    }

    // helper: fire-and-forget safety
    static class TaskExtensions
    {
        public static void Forget(this Task t) { _ = t.ContinueWith(_ => { }); }
    }
}
