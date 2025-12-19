namespace NitroSystem.Dnn.BusinessEngine.Core.DSLEngine.Enums
{
    public enum TokenType
    {
        Identifier,
        Number,
        String,
        Boolean,

        If,
        Else,
        Begin,
        End,

        Equals,
        DoubleEquals,
        NotEquals,
        AndAnd,
        OrOr,

        Dot,
        Semicolon,
        Comma,
        OpenParen,
        CloseParen,

        EndOfFile
    }
}
