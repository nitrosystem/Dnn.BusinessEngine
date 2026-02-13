using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine
{
    public sealed class DslParser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public DslParser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token Current => _tokens[_pos];
        private Token Consume() => _tokens[_pos++];

        private bool Match(TokenType type)
        {
            if (Current.Type != type) return false;
            _pos++;
            return true;
        }

        private Token Expect(TokenType type)
        {
            if (Current.Type != type)
                throw new InvalidOperationException("Expected: " + type);
            return Consume();
        }

        // ===================== Script =====================

        public DslScript ParseScript()
        {
            var statements = new List<DslStatement>();

            while (Current.Type != TokenType.EndOfFile)
                statements.Add(ParseStatement());

            return new DslScript
            {
                Version = "1.0",
                Statements = statements
            };
        }

        private DslStatement ParseStatement()
        {
            if (Current.Type == TokenType.If)
                return ParseIf();

            if (Current.Type == TokenType.Identifier)
            {
                if (_tokens[_pos + 1].Type == TokenType.OpenParen)
                    return ParseFunctionCall();

                return ParseAssignment();
            }

            throw new InvalidOperationException("Invalid statement");
        }

        // ===================== If =====================

        private IfStatement ParseIf()
        {
            Expect(TokenType.If);
            var condition = ParseExpression();

            Expect(TokenType.Begin);
            var thenList = new List<DslStatement>();

            while (Current.Type != TokenType.End)
                thenList.Add(ParseStatement());

            Expect(TokenType.End);

            List<DslStatement> elseList = null;
            if (Match(TokenType.Else))
            {
                Expect(TokenType.Begin);
                elseList = new List<DslStatement>();

                while (Current.Type != TokenType.End)
                    elseList.Add(ParseStatement());

                Expect(TokenType.End);
            }

            return new IfStatement
            {
                Condition = condition,
                Then = thenList,
                Else = elseList
            };
        }

        // ===================== Assignment =====================

        private AssignmentStatement ParseAssignment()
        {
            var target = ParseMemberAccess();
            Expect(TokenType.Equals);
            var value = ParseExpression();
            //Expect(TokenType.Semicolon);

            return new AssignmentStatement
            {
                Target = target,
                Value = value
            };
        }

        // ===================== Function Call =====================

        private FunctionCallStatement ParseFunctionCall()
        {
            string name = Expect(TokenType.Identifier).Value;
            Expect(TokenType.OpenParen);

            var args = new List<DslExpression>();
            if (Current.Type != TokenType.CloseParen)
            {
                do
                {
                    args.Add(ParseExpression());
                }
                while (Match(TokenType.Comma));
            }

            Expect(TokenType.CloseParen);
            //Expect(TokenType.Semicolon);

            return new FunctionCallStatement
            {
                FunctionName = name,
                Arguments = args
            };
        }

        // =======================================================
        // ===================== EXPRESSIONS =====================
        // =======================================================

        // Entry point
        private DslExpression ParseExpression()
        {
            return ParseOr();
        }

        // OR (||)
        private DslExpression ParseOr()
        {
            var left = ParseAnd();

            while (Match(TokenType.OrOr))
            {
                var right = ParseAnd();
                left = new BinaryExpression
                {
                    Left = left,
                    Operator = "||",
                    Right = right
                };
            }

            return left;
        }

        // AND (&&)
        private DslExpression ParseAnd()
        {
            var left = ParseEquality();

            while (Match(TokenType.AndAnd))
            {
                var right = ParseEquality();
                left = new BinaryExpression
                {
                    Left = left,
                    Operator = "&&",
                    Right = right
                };
            }

            return left;
        }

        // == , !=
        private DslExpression ParseEquality()
        {
            var left = ParsePrimary();

            while (Current.Type == TokenType.DoubleEquals ||
                   Current.Type == TokenType.NotEquals ||
                   Current.Type == TokenType.Bigger ||
                   Current.Type == TokenType.Smaller)
            {
                string op = Consume().Value;
                var right = ParsePrimary();

                left = new BinaryExpression
                {
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        // ===================== Primary =====================

        private DslExpression ParsePrimary()
        {
            if (Current.Type == TokenType.String)
            {
                return new LiteralExpression
                {
                    Value = Consume().Value
                };
            }

            if (Current.Type == TokenType.Boolean)
            {
                return new LiteralExpression
                {
                    Value = bool.Parse(Consume().Value)
                };
            }

            if (Current.Type == TokenType.Number)
            {
                return new LiteralExpression
                {
                    Value = int.Parse(Consume().Value)
                };
            }

            if (Current.Type == TokenType.Identifier)
            {
                // Lookahead
                if (_tokens[_pos + 1].Type == TokenType.OpenParen)
                    return ParseFunctionCallExpression();

                return ParseMemberAccess();
            }


            throw new InvalidOperationException("Invalid expression");
        }

        private MemberAccessExpression ParseMemberAccess()
        {
            var path = new List<string>
        {
            Expect(TokenType.Identifier).Value
        };

            while (Match(TokenType.Dot))
                path.Add(Expect(TokenType.Identifier).Value);

            return new MemberAccessExpression
            {
                Path = path
            };
        }

        private DslExpression ParseFunctionCallExpression()
        {
            string name = Expect(TokenType.Identifier).Value;
            Expect(TokenType.OpenParen);

            var args = new List<DslExpression>();
            if (Current.Type != TokenType.CloseParen)
            {
                do
                {
                    args.Add(ParseExpression());
                }
                while (Match(TokenType.Comma));
            }

            Expect(TokenType.CloseParen);

            return new FunctionCallExpression
            {
                FunctionName = name,
                Arguments = args
            };
        }
    }

}
