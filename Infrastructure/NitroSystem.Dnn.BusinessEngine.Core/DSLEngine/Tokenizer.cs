using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Enums;
using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine
{
    public sealed class Tokenizer
    {
        private readonly string _text;
        private int _pos;

        public Tokenizer(string text)
        {
            _text = text;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (!IsEnd())
            {
                char c = Peek();

                if (char.IsWhiteSpace(c))
                {
                    _pos++;
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    tokens.Add(ReadIdentifier());
                    continue;
                }

                if (char.IsDigit(c))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                switch (c)
                {
                    case '=':
                        if (Peek(1) == '=')
                        {
                            Advance(); // =
                            Advance(); // =
                            tokens.Add(new Token(TokenType.DoubleEquals, "==", _pos - 2));
                        }
                        else
                        {
                            Advance();
                            tokens.Add(new Token(TokenType.Equals, "=", _pos - 1));
                        }
                        break;

                    case '!':
                        if (Peek(1) != '=')
                            throw new InvalidOperationException("Expected '=' after '!'");
                        Advance();
                        Advance();
                        tokens.Add(new Token(TokenType.NotEquals, "!=", _pos - 2));
                        break;

                    case '&':
                        if (Peek(1) != '&')
                            throw new InvalidOperationException("Expected '&&'");
                        Advance();
                        Advance();
                        tokens.Add(new Token(TokenType.AndAnd, "&&", _pos - 2));
                        break;

                    case '|':
                        if (Peek(1) != '|')
                            throw new InvalidOperationException("Expected '||'");
                        Advance();
                        Advance();
                        tokens.Add(new Token(TokenType.OrOr, "||", _pos - 2));
                        break;

                    case '"':
                        tokens.Add(ReadString());
                        break;

                    case '.': tokens.Add(Simple(TokenType.Dot)); break;
                    //case ';': tokens.Add(Simple(TokenType.Semicolon)); break;
                    case ',': tokens.Add(Simple(TokenType.Comma)); break;
                    case '(': tokens.Add(Simple(TokenType.OpenParen)); break;
                    case ')': tokens.Add(Simple(TokenType.CloseParen)); break;

                    default:
                        throw new InvalidOperationException("Unexpected char: " + c);
                }
            }

            tokens.Add(new Token(TokenType.EndOfFile, null, _pos));
            return tokens;
        }

        private Token Simple(TokenType type)
        {
            return new Token(type, _text[_pos++].ToString(), _pos - 1);
        }

        private bool Match(char expected)
        {
            if (Peek() != expected) return false;
            _pos++;
            return true;
        }

        private void Expect(char c)
        {
            if (Peek() != c)
                throw new InvalidOperationException("Expected: " + c);
            _pos++;
            _pos++;
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

                // Escape ساده (فعلاً)
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
                case "true":
                case "false": return new Token(TokenType.Boolean, value, start);
            }

            return new Token(TokenType.Identifier, value, start);
        }

        private Token ReadNumber()
        {
            int start = _pos;
            while (!IsEnd() && char.IsDigit(Peek()))
                _pos++;

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

        //private char Peek() => _pos >= _text.Length ? '\0' : _text[_pos];
        private bool IsEnd() => _pos >= _text.Length;
    }

}
