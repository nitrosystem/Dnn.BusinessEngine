using System;
using System.Text;
using System.Collections.Generic;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Models;

namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine
{
    public sealed class Tokenizer
    {
        private readonly string _text;
        private int _pos;
        private TokenType? _lastTokenType;

        public Tokenizer(string text)
        {
            _text = text;
        }

        /// <summary>
        /// Eager tokenization — unchanged public contract, still returns a List&lt;Token&gt;.
        /// Existing callers (full DSL script parsing) are unaffected.
        /// </summary>
        public List<Token> Tokenize() => new List<Token>(TokenizeCore());

        /// <summary>
        /// NEW: lazy, pull-based tokenization. Consumers (like DslParser fed from
        /// TemplateParser's inline-expression boundary detection) only pull as many
        /// tokens as they actually need — the Tokenizer never has to process, and
        /// potentially throw on, text beyond that point (e.g. surrounding HTML that
        /// isn't valid DSL syntax at all).
        /// </summary>
        public IEnumerable<Token> TokenizeLazy() => TokenizeCore();

        private IEnumerable<Token> TokenizeCore()
        {
            bool stopped = false;

            while (!stopped && !IsEnd())
            {
                char c = Peek();

                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    yield return Emit(ReadIdentifier());
                    continue;
                }

                if (char.IsDigit(c))
                {
                    yield return Emit(ReadNumber());
                    continue;
                }

                switch (c)
                {
                    case '+':
                        if (IsAcceptable())
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Add, "+", _pos - 1));
                        }
                        break;
                    case '-':
                        if (IsAcceptable())
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Subtract, "-", _pos - 1));
                        }
                        else if (char.IsDigit(Peek(1)))
                            yield return Emit(ReadNumber(true));
                        break;
                    case '*':
                        if (IsAcceptable())
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Multiply, "*", _pos - 1));
                        }
                        break;
                    case '/':
                        if (IsAcceptable())
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Divide, "/", _pos - 1));
                        }
                        break;
                    case '=':
                        if (Peek(1) == '=')
                        {
                            Advance(); // =
                            Advance(); // =
                            yield return Emit(new Token(TokenType.DoubleEquals, "==", _pos - 2));
                        }
                        else
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Equals, "=", _pos - 1));
                        }
                        break;

                    case '!':
                        if (Peek(1) == '=')
                        {
                            Advance();
                            Advance();
                            yield return Emit(new Token(TokenType.NotEquals, "!=", _pos - 2));
                        }
                        else
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Not, "!", _pos - 1));
                        }
                        break;
                    case '>':
                        if (Peek(1) == '=')
                        {
                            Advance(); // >
                            Advance(); // =
                            yield return Emit(new Token(TokenType.BiggerThenEquals, ">=", _pos - 2));
                        }
                        else
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Bigger, ">", _pos - 1));
                        }
                        break;
                    case '<':
                        if (Peek(1) == '=')
                        {
                            Advance(); // <
                            Advance(); // =
                            yield return Emit(new Token(TokenType.SmallerThenEquals, "<=", _pos - 2));
                        }
                        else
                        {
                            Advance();
                            yield return Emit(new Token(TokenType.Smaller, "<", _pos - 1));
                        }
                        break;

                    case '"':
                        yield return Emit(ReadString());
                        break;

                    case '.': yield return Emit(Simple(TokenType.Dot)); break;
                    case ',': yield return Emit(Simple(TokenType.Comma)); break;
                    case '(': yield return Emit(Simple(TokenType.OpenParen)); break;
                    case ')': yield return Emit(Simple(TokenType.CloseParen)); break;

                    default:
                        // NEW: '#' never appears in valid DSL syntax. When this Tokenizer
                        // is used to find the boundary of an inline expression embedded in
                        // template text (e.g. "#If cond#End" or "#If cond and-more-html"),
                        // a directive marker showing up right after an expression is a
                        // completely ordinary, valid boundary — not a syntax error. Stop
                        // cleanly here instead of throwing; _pos is left exactly at '#',
                        // so the EndOfFile token below carries that exact position.
                        if (c == '#')
                        {
                            stopped = true;
                            break;
                        }
                        throw new InvalidOperationException("Unexpected char: " + c);
                }
            }

            yield return new Token(TokenType.EndOfFile, null, _pos);
        }

        private Token Emit(Token token)
        {
            _lastTokenType = token.Type;
            return token;
        }

        private Token Simple(TokenType type)
        {
            return new Token(type, _text[_pos++].ToString(), _pos - 1);
        }

        private Token ReadString()
        {
            int start = _pos;
            _pos++; // skip opening "

            var sb = new StringBuilder();

            while (!IsEnd())
            {
                char c = Peek();

                if (c == '"')
                {
                    _pos++; // skip closing "
                    break;
                }

                // Escape 
                if (c == '\\')
                {
                    _pos++;
                    char escaped = Peek();
                    sb.Append(escaped);
                    _pos++;
                    continue;
                }

                sb.Append(c);
                _pos++;
            }

            return new Token(TokenType.String, sb.ToString(), start);
        }

        private Token ReadIdentifier()
        {
            int start = _pos;
            while (!IsEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                _pos++;

            string value = _text.Substring(start, _pos - start);

            switch (value)
            {
                case "if": return new Token(TokenType.If, value, start);
                case "else": return new Token(TokenType.Else, value, start);
                case "begin": return new Token(TokenType.Begin, value, start);
                case "end": return new Token(TokenType.End, value, start);
                case "and": return new Token(TokenType.And, value, start);
                case "or": return new Token(TokenType.Or, value, start);
                case "true":
                case "false": return new Token(TokenType.Boolean, value, start);
                case "null": return new Token(TokenType.Null, value, start);
            }

            return new Token(TokenType.Identifier, value, start);
        }

        private Token ReadNumber(bool includeDash = false)
        {
            int start = _pos;

            if (includeDash) _pos++;

            while (!IsEnd() && char.IsDigit(Peek()))
                _pos++;

            // Decimal part — only consume the dot if followed by a digit, so a plain
            // "." used for member access is never swallowed by mistake.
            if (!IsEnd() && Peek() == '.' && char.IsDigit(Peek(1)))
            {
                _pos++; // consume '.'
                while (!IsEnd() && char.IsDigit(Peek()))
                    _pos++;
            }

            return new Token(TokenType.Number,
                _text.Substring(start, _pos - start), start);
        }

        private char Peek(int offset = 0)
        {
            int index = _pos + offset;
            return index >= _text.Length ? '\0' : _text[index];
        }

        private char Advance()
        {
            return _text[_pos++];
        }

        private bool IsAcceptable()
        {
            if (_lastTokenType is null) return false;

            switch (_lastTokenType.Value)
            {
                case TokenType.Identifier:
                case TokenType.Number:
                case TokenType.String:
                case TokenType.Boolean:
                case TokenType.CloseParen:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsEnd() => _pos >= _text.Length;
    }
}
