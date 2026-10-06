using System.Text;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes
{
    public class ForNode : Node
    {
        public string ItemName { get; }
        public string CollectionName { get; }
        public List<Node> Children { get; } = new();

        public ForNode(string itemName, string collectionName)
        {
            ItemName = itemName;
            CollectionName = collectionName;
        }

        public override void Render(StringBuilder sb, TemplateContext context)
        {
            // CHANGED: CollectionName is now a full DSL expression (e.g. "order.Items"),
            // evaluated through the shared pipeline instead of a flat TryGet lookup.
            var collection = TemplateExpressionEngine.Evaluate(CollectionName, context);

            // CHANGED: widened from IEnumerable<object> to plain IEnumerable so
            // value-type collections (List<int>, List<string>, ...) aren't silently
            // skipped — IEnumerable<object> only worked before due to reference-type
            // covariance.
            if (collection is System.Collections.IEnumerable items)
            {
                foreach (var item in items)
                {
                    context.PushScope(ItemName, item);
                    foreach (var child in Children)
                        child.Render(sb, context);
                    context.PopScope();
                }
            }
        }
    }
}
