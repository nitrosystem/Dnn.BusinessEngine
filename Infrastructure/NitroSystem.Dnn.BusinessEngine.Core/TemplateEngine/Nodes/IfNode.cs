using System;
using System.Text;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes
{
    public class IfNode : Node
    {
        public string Condition { get; }
        public List<Node> TrueBranch { get; } = new();
        public List<Node> FalseBranch { get; } = new();
        public bool HasElse { get; private set; }

        public IfNode(string condition) => Condition = condition;

        public void SwitchToElse()
        {
            if (HasElse) throw new InvalidOperationException("Multiple #Else in same #If");
            HasElse = true;
        }

        public List<Node> CurrentBranch => HasElse ? FalseBranch : TrueBranch;

        public override void Render(StringBuilder sb, TemplateContext context)
        {
            // CHANGED: was ExpressionEvaluator.Evaluate(Condition, context) from
            // TemplateEngine.Expressions (the old, parallel, template-only evaluator).
            // That namespace/class is now dead code — safe to delete once you've
            // confirmed nothing else in the codebase references it.
            var result = (bool)TemplateExpressionEngine.Evaluate(Condition, context);
            var branchToRender = result ? TrueBranch : FalseBranch;

            foreach (var child in branchToRender)
                child.Render(sb, context);
        }
    }
}
