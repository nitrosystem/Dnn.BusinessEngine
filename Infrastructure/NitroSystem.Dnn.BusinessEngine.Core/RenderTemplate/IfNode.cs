using System.Text;
using System.Collections.Generic;
using System;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
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

        public override void Render(StringBuilder sb, RenderContext context)
        {
            bool result = ExpressionEvaluator.Evaluate(Condition, context);
            var branchToRender = result ? TrueBranch : FalseBranch;

            foreach (var child in branchToRender)
                child.Render(sb, context);
        }
    }
}