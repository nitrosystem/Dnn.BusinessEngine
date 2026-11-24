using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine;
using NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine.Nodes;

namespace NitroSystem.Dnn.BusinessEngine.Core.TemplateEngine
{
    public class TemplateParser
    {
        private static readonly Regex DirectiveStart = new Regex(
            @"#(For|If|Else|End|Function)\b",
            RegexOptions.IgnoreCase);

        public RootNode Parse(string template)
        {
            var root = new RootNode();
            var stack = new Stack<BlockContext>();
            stack.Push(new BlockContext(root, root.Children));

            var lines = template.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var rawLine in lines)
                ParseLine(rawLine, stack);

            if (stack.Count != 1)
                throw new InvalidOperationException("Unclosed block detected");

            return root;
        }

        // CHANGED: was a single "find the first directive on this line, treat
        // everything after it as that directive's own content" pass. That broke
        // as soon as more than one directive landed on the same physical line
        // (e.g. a minified/inlined "#If cond text-white #End" all on one line) —
        // the #End never got seen as its own directive, so the block never
        // closed, surfacing as "Unclosed block detected".
        //
        // Now this loops within the line, handling every directive it finds in
        // order, and only stops when nothing further matches.
        private void ParseLine(string rawLine, Stack<BlockContext> stack)
        {
            int pos = 0;
            bool matchedAny = false;

            while (true)
            {
                var match = DirectiveStart.Match(rawLine, pos);

                if (!match.Success)
                {
                    var remainder = rawLine.Substring(pos);

                    if (!matchedAny)
                    {
                        // Pure text line — always preserve, blank lines included
                        // (same as the original behavior).
                        stack.Peek().Children.Add(new TextNode(remainder + Environment.NewLine));
                    }
                    else if (remainder.Length > 0)
                    {
                        // Trailing text after the last directive on this line — this
                        // really is the end of the physical source line, so restore
                        // its line break.
                        stack.Peek().Children.Add(new TextNode(remainder + Environment.NewLine));
                    }
                    // else: the line was fully consumed by directive(s) — drop its
                    // line break, so "#If .../ #End"-only lines don't leave blank
                    // lines in the rendered output (matches the original single-
                    // directive-per-line behavior).
                    return;
                }

                matchedAny = true;

                if (match.Index > pos)
                {
                    var prefix = rawLine.Substring(pos, match.Index - pos);
                    stack.Peek().Children.Add(new TextNode(prefix));
                }

                var keyword = match.Groups[1].Value.ToLowerInvariant();
                var afterKeyword = match.Index + match.Length;

                switch (keyword)
                {
                    case "for":
                        {
                            var node = ParseFor(rawLine, afterKeyword, out pos);
                            stack.Peek().Children.Add(node);
                            stack.Push(new BlockContext(node, node.Children));
                            break;
                        }

                    case "if":
                        {
                            var node = ParseIf(rawLine, afterKeyword, out pos);
                            stack.Peek().Children.Add(node);
                            stack.Push(new BlockContext(node, node.TrueBranch));
                            break;
                        }

                    case "else":
                        {
                            HandleElse(stack);
                            pos = afterKeyword;
                            break;
                        }

                    case "end":
                        {
                            if (stack.Count == 1)
                                throw new InvalidOperationException("Unexpected #End");
                            stack.Pop();
                            pos = afterKeyword;
                            break;
                        }

                    case "function":
                        {
                            var node = ParseFunction(rawLine, afterKeyword, out pos);
                            stack.Peek().Children.Add(node);
                            break;
                        }
                }
            }
        }

        private ForNode ParseFor(string line, int start, out int endPos)
        {
            // Fixed prefix: "  itemName  in  " — everything after that is a full
            // DSL expression, whose exact end is found via ExtractExpression
            // rather than assumed to run to the end of the line.
            var prefixMatch = Regex.Match(line.Substring(start), @"^\s*(\w+)\s+in\s+", RegexOptions.IgnoreCase);
            if (!prefixMatch.Success)
                throw new InvalidOperationException($"Invalid For syntax: {line}");

            var itemName = prefixMatch.Groups[1].Value;
            var collectionStart = start + prefixMatch.Length;

            var (collectionText, consumedLength) = ExtractExpression(line.Substring(collectionStart));
            endPos = collectionStart + consumedLength;

            return new ForNode(itemName, collectionText);
        }

        private IfNode ParseIf(string line, int start, out int endPos)
        {
            var (conditionText, consumedLength) = ExtractExpression(line.Substring(start));
            if (string.IsNullOrWhiteSpace(conditionText))
                throw new InvalidOperationException("Empty If condition");

            endPos = start + consumedLength;
            return new IfNode(conditionText);
        }

        /// <summary>
        /// Parses exactly one DSL expression from the start of <paramref name="source"/>
        /// (which may have arbitrary, non-DSL text after it — e.g. more template
        /// literal HTML, or the next "#..." directive) and returns that expression's
        /// text plus how many characters of <paramref name="source"/> it consumed.
        ///
        /// This is what lets "#If State == "get-started" text-white #End" work:
        /// parsing stops the instant the expression grammar can't extend any
        /// further (here, right after the closing quote — "text-white" isn't a
        /// valid continuation), without the Tokenizer ever having to look at, or
        /// choke on, "#End" and beyond.
        /// </summary>
        private static (string Text, int Length) ExtractExpression(string source)
        {
            var tokens = new Tokenizer(source).TokenizeLazy();
            var parser = new DslParser(tokens);
            parser.ParseExpressionEntry();

            var length = parser.CurrentPosition;
            return (source.Substring(0, length).Trim(), length);
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

        private FunctionNode ParseFunction(string line, int start, out int endPos)
        {
            // NOTE: kept as its own regex-based extraction (not routed through
            // ExtractExpression) since FunctionNode's exact shape wasn't available
            // when this was written — flag if #Function ever needs the same
            // same-line-as-other-directives robustness #For/#If just got.
            var remainder = line.Substring(start);
            var match = Regex.Match(remainder, @"^\s*(\w+)\s*\(\s*(.+?)\s*\)", RegexOptions.IgnoreCase);
            if (!match.Success) throw new InvalidOperationException($"Invalid Function syntax: {line}");

            endPos = start + match.Index + match.Length;
            return new FunctionNode(match.Groups[1].Value, match.Groups[2].Value);
        }
    }
}
