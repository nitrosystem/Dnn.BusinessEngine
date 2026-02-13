namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Enums
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

        Bigger,
        Smaller,

        Dot,
        Semicolon,
        Comma,
        OpenParen,
        CloseParen,

        EndOfFile
    }
}
