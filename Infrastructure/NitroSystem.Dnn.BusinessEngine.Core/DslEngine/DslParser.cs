using System;
using System.Collections.Generic;
using System.Globalization;
using NitroSystem.Dnn.BusinessEngine.Core.ExpressionEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Base;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Expressions;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Statements;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine
{
    public sealed class DslParser
    {
        // CHANGED: was `List<Token> _tokens` with direct indexing (_tokens[_pos]).
        // Now pulls lazily from any IEnumerable<Token> (including Tokenizer.TokenizeLazy()),
        // buffering only as many tokens as parsing actually demands. A List<Token>
        // (from the existing eager Tokenize()) still works fine here — List<T>
        // implements IEnumerable<T> — so this is backward compatible for every
        // existing call site.
        private readonly IEnumerator<Token> _source;
        private readonly List<Token> _buffer = new();
        private bool _sourceExhausted;
        private int _pos;

        public DslParser(IEnumerable<Token> tokens)
        {
            _source = tokens.GetEnumerator();
        }

        private void EnsureBuffered(int index)
        {
            while (_buffer.Count <= index && !_sourceExhausted)
            {
                if (_source.MoveNext())
                    _buffer.Add(_source.Current);
                else
                    _sourceExhausted = true;
            }
        }

        private Token TokenAt(int index)
        {
            EnsureBuffered(index);
            // Past the real end of the stream, keep handing back the last buffered
            // token (always EndOfFile — Tokenizer guarantees exactly one trailing
            // EndOfFile) so a lookahead right at EOF never indexes out of range.
            return index < _buffer.Count ? _buffer[index] : _buffer[_buffer.Count - 1];
        }

        private Token Current => TokenAt(_pos);
        private Token Peek(int offset) => TokenAt(_pos + offset);

        private Token Consume()
        {
            var t = Current;
            _pos++;
            return t;
        }

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

        /// <summary>
        /// NEW: character offset (in the original source text handed to the
        /// Tokenizer) of the next token the parser hasn't consumed yet — i.e. where
        /// whatever was just parsed ends. Used by TemplateParser to know exactly
        /// where an inline "#If ..."/"#For ... in ..." expression stops and
        /// surrounding template text resumes.
        /// </summary>
        public int CurrentPosition => Current.Position;

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

        // Public entry point for parsing a standalone expression (no
        // statement/script wrapper) — used by consumers like TemplateEngine
        // that only need expression evaluation (a #For collection path, a
        // #If condition), not full DSL script statements.
        public ExpressionBase ParseExpressionEntry() => ParseExpression();

        private DslStatement ParseStatement()
        {
            if (Current.Type == TokenType.If)
                return ParseIf();

            if (Current.Type == TokenType.Identifier)
            {
                if (Peek(1).Type == TokenType.OpenParen)
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

            var args = new List<ExpressionBase>();
            if (Current.Type != TokenType.CloseParen)
            {
                do
                {
                    args.Add(ParseExpression());
                }
                while (Match(TokenType.Comma));
            }

            Expect(TokenType.CloseParen);

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
        private ExpressionBase ParseExpression()
        {
            return ParseOr();
        }

        // OR 
        private ExpressionBase ParseOr()
        {
            var left = ParseAnd();

            while (Match(TokenType.Or))
            {
                var right = ParseAnd();
                left = new BinaryExpression
                {
                    Left = left,
                    Operator = "or",
                    Right = right
                };
            }

            return left;
        }

        // AND 
        private ExpressionBase ParseAnd()
        {
            var left = ParseEquality();

            while (Match(TokenType.And))
            {
                var right = ParseEquality();
                left = new BinaryExpression
                {
                    Left = left,
                    Operator = "and",
                    Right = right
                };
            }

            return left;
        }

        // == , !=, >, >=, <, <=, +, -, *, /
        private ExpressionBase ParseEquality()
        {
            var left = ParseUnary();

            while (Current.Type == TokenType.DoubleEquals ||
                   Current.Type == TokenType.NotEquals ||
                   Current.Type == TokenType.Bigger ||
                   Current.Type == TokenType.BiggerThenEquals ||
                   Current.Type == TokenType.Smaller ||
                   Current.Type == TokenType.SmallerThenEquals ||
                   Current.Type == TokenType.Add ||
                   Current.Type == TokenType.Subtract ||
                   Current.Type == TokenType.Multiply ||
                   Current.Type == TokenType.Divide)
            {
                string op = Consume().Value;
                var right = ParseUnary();

                left = new BinaryExpression
                {
                    Left = left,
                    Operator = op,
                    Right = right
                };
            }

            return left;
        }

        // Unary "!" (logical negation). Note: unary minus (e.g. "-order.Total") is
        // still not supported here — Subtract is only ever parsed as a binary
        // operator above. Only a leading numeric literal like "-5" works, via the
        // Tokenizer's includeDash heuristic.
        private ExpressionBase ParseUnary()
        {
            if (Match(TokenType.Not))
            {
                var operand = ParseUnary(); // allows "!!x" too, harmless
                return new UnaryExpression
                {
                    Operator = "!",
                    Operand = operand
                };
            }

            return ParsePrimary();
        }

        // ===================== Primary =====================
        private ExpressionBase ParsePrimary()
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
                var raw = Consume().Value;
                object value = raw.Contains(".")
                    ? (object)double.Parse(raw, CultureInfo.InvariantCulture)
                    : int.Parse(raw, CultureInfo.InvariantCulture);

                return new LiteralExpression { Value = value };
            }

            if (Current.Type == TokenType.Null)
            {
                Consume();
                return new LiteralExpression
                {
                    Value = null
                };
            }

            if (Current.Type == TokenType.Identifier)
            {
                // Lookahead
                if (Peek(1).Type == TokenType.OpenParen)
                    return ParseFunctionCallExpression();

                return ParseMemberAccess();
            }

            throw new InvalidOperationException("Invalid expression");
        }

        private MemberAccessExpression ParseMemberAccess()
        {
            var path = new List<string> { Expect(TokenType.Identifier).Value };
            while (Match(TokenType.Dot))
                path.Add(Expect(TokenType.Identifier).Value);

            return new MemberAccessExpression
            {
                Path = path
            };
        }

        private ExpressionBase ParseFunctionCallExpression()
        {
            string name = Expect(TokenType.Identifier).Value;
            Expect(TokenType.OpenParen);

            var args = new List<ExpressionBase>();
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
