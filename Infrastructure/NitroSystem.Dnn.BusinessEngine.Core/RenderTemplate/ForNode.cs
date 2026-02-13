using System.Text;
using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
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

        public override void Render(StringBuilder sb, RenderContext context)
        {
            if (!context.TryGet(CollectionName, out var collection))
                return;

            if (collection is IEnumerable<object> items)
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