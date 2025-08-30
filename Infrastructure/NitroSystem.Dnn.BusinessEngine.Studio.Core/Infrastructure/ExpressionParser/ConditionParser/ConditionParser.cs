using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.ExpressionService.ConditionParser
{
    public class ConditionParser
    {
        public static ConditionNode Parse(string condition)
        {
            var tokens = Tokenize(condition);
            return BuildExpressionTree(tokens);
        }

        private static List<string> Tokenize(string condition)
        {
            var LogicalOperators = new HashSet<string> { "&&", "||" };
            var ComparisonOperators = new HashSet<string> { "==", "!=", ">", "<", ">=", "<=" };

            var tokens = new List<string>();
            var currentToken = "";

            for (int i = 0; i < condition.Length; i++)
            {
                char c = condition[i];

                if (char.IsWhiteSpace(c)) continue;

                if (c == '(' || c == ')')
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(currentToken);
                        currentToken = "";
                    }
                    tokens.Add(c.ToString());
                    continue;
                }

                if (c == '&' || c == '|')
                {
                    if (i + 1 < condition.Length && condition[i + 1] == c)
                    {
                        if (!string.IsNullOrEmpty(currentToken))
                        {
                            tokens.Add(currentToken);
                            currentToken = "";
                        }
                        tokens.Add(new string(c, 2));
                        i++;
                        continue;
                    }
                }

                if (ComparisonOperators.Any(op => op.StartsWith(c.ToString())))
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(currentToken);
                        currentToken = "";
                    }

                    string op = c.ToString();
                    if (i + 1 < condition.Length && ComparisonOperators.Contains(op + condition[i + 1]))
                    {
                        op += condition[i + 1];
                        i++;
                    }

                    tokens.Add(op);
                    continue;
                }

                currentToken += c;
            }

            if (!string.IsNullOrEmpty(currentToken))
            {
                tokens.Add(currentToken);
            }

            return tokens;
        }

        private static ConditionNode BuildExpressionTree(List<string> tokens)
        {
            var LogicalOperators = new HashSet<string> { "&&", "||" };
            var ComparisonOperators = new HashSet<string> { "==", "!=", ">", "<", ">=", "<=" };

            var stack = new Stack<ConditionNode>();
            var operatorStack = new Stack<string>();

            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                if (token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    while (operatorStack.Peek() != "(")
                    {
                        stack.Push(CreateLogicalNode(operatorStack.Pop(), stack.Pop(), stack.Pop()));
                    }
                    operatorStack.Pop();
                }
                else if (LogicalOperators.Contains(token))
                {
                    while (operatorStack.Count > 0 && LogicalOperators.Contains(operatorStack.Peek()))
                    {
                        stack.Push(CreateLogicalNode(operatorStack.Pop(), stack.Pop(), stack.Pop()));
                    }
                    operatorStack.Push(token);
                }
                else if (ComparisonOperators.Contains(token))
                {
                    var left = stack.Pop();
                    var right = tokens[++i];
                    stack.Push(new ComparisonNode
                    {
                        FieldName = ((ComparisonNode)left).FieldName,
                        Operator = token,
                        Value = right
                    });
                }
                else
                {
                    stack.Push(new ComparisonNode { FieldName = token });
                }
            }

            while (operatorStack.Count > 0)
            {
                stack.Push(CreateLogicalNode(operatorStack.Pop(), stack.Pop(), stack.Pop()));
            }

            return stack.Pop();
        }

        private static LogicalNode CreateLogicalNode(string op, ConditionNode right, ConditionNode left)
        {
            return new LogicalNode
            {
                Operator = op,
                Left = left,
                Right = right
            };
        }
    }
}
