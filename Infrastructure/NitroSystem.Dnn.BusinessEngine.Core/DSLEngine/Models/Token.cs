using NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Enums;

namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Models
{
    public sealed class Token
    {
        public TokenType Type { get; private set; }
        public string Value { get; private set; }
        public int Position { get; private set; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }
    }
}
