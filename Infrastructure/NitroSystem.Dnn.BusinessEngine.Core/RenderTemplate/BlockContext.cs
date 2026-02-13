using System.Collections.Generic;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public class BlockContext
    {
        public Node Owner { get; }
        public List<Node> Children { get; }

        public BlockContext(Node owner, List<Node> children)
        {
            Owner = owner;
            Children = children;
        }
    }
}
