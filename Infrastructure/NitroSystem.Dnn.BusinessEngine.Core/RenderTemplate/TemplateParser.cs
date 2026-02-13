using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NitroSystem.Dnn.BusinessEngine.Core.RenderTemplate
{
    public class TemplateParser
    {
        public RootNode Parse(string template)
        {
            var root = new RootNode();
            var stack = new Stack<BlockContext>();
            stack.Push(new BlockContext(root, root.Children));

            var lines = template.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimStart();

                if (line.StartsWith("#For "))
                {
                    var node = ParseFor(line);
                    stack.Peek().Children.Add(node);
                    stack.Push(new BlockContext(node, node.Children));
                    continue;
                }

                if (line.StartsWith("#If "))
                {
                    var node = ParseIf(line);
                    stack.Peek().Children.Add(node);
                    stack.Push(new BlockContext(node, node.TrueBranch));
                    continue;
                }

                if (line.StartsWith("#Else"))
                {
                    HandleElse(stack);
                    continue;
                }

                if (line.StartsWith("#End"))
                {
                    if (stack.Count == 1)
                        throw new InvalidOperationException("Unexpected #End");
                    stack.Pop();
                    continue;
                }

                if (line.StartsWith("#Function"))
                {
                    var node = ParseFunctions(line);
                    stack.Peek().Children.Add(node);
                    continue;
                }

                stack.Peek().Children.Add(new TextNode(rawLine + Environment.NewLine));
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("Unclosed block detected");

            return root;
        }

        private ForNode ParseFor(string line)
        {
            var match = Regex.Match(line, @"#For\s+(\w+)\s+in\s+(\w+)", RegexOptions.IgnoreCase);
            if (!match.Success) throw new InvalidOperationException($"Invalid For syntax: {line}");
            return new ForNode(match.Groups[1].Value, match.Groups[2].Value);
        }

        private IfNode ParseIf(string line)
        {
            var condition = line.Substring(3).Trim();
            if (string.IsNullOrWhiteSpace(condition))
                throw new InvalidOperationException("Empty If condition");
            return new IfNode(condition);
        }

        private void HandleElse(Stack<BlockContext> stack)
        {
            if (stack.Count < 2) throw new InvalidOperationException("#Else without #If");

            var current = stack.Pop();
            var parent = stack.Peek();

            if (current.Owner is not IfNode ifNode)
                throw new InvalidOperationException("#Else must be inside #If");

            ifNode.SwitchToElse();
            stack.Push(new BlockContext(ifNode, ifNode.FalseBranch));
        }

        private FunctionNode ParseFunctions(string line)
        {
            var match = Regex.Match(line, @"#Function\s+(\w+)\s*\(\s*(.+)\s*\)", RegexOptions.IgnoreCase);
            if (!match.Success) throw new InvalidOperationException($"Invalid Function syntax: {line}");
            return new FunctionNode(match.Groups[1].Value, match.Groups[2].Value);
        }

        private class BlockContext
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
}