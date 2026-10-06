using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine
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
