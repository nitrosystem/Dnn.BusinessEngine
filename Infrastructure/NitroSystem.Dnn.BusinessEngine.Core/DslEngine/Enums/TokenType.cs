namespace NitroSystem.Dnn.BusinessEngine.Core.DslEngine.Enums
{
    public enum TokenType
    {
        Identifier,
        Number,
        String,
        Boolean,
        Null,

        If,
        Else,
        Begin,
        End,

        Equals,
        DoubleEquals,
        NotEquals,
        Not,           // NEW: standalone "!" (unary negation), distinct from "!="
        And,
        Or,

        Bigger,
        BiggerThenEquals,
        Smaller,
        SmallerThenEquals,

        Add,
        Subtract,
        Multiply,
        Divide,

        Dot,
        Comma,
        OpenParen,
        CloseParen,

        EndOfFile
    }
}
