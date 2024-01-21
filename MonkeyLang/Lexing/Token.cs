namespace MonkeyLang.Lexing;

public enum TokenType
{
    Illegal,
    Eof,

    // Identifiers + literals
    Ident,
    Int,

    // Operators
    Assign,
    Plus,
    Minus,
    Bang,
    Asterisk,
    Slash,

    Lt,
    Gt,
    Eq,
    NotEq,

    // Delimiters
    Comma,
    Semicolon,
    Lparen,
    Rparen,
    Lbrace,
    Rbrace,

    // Keywords
    Function,
    Let,
    True,
    False,
    If,
    Else,
    Return
}

public struct Token(TokenType tokenType, string literal)
{
    public readonly TokenType TokenType = tokenType;
    public readonly string Literal = literal;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "fn", TokenType.Function },
        { "let", TokenType.Let },
        { "true", TokenType.True },
        { "false", TokenType.False },
        { "if", TokenType.If },
        { "else", TokenType.Else },
        { "return", TokenType.Return }
    };

    public static TokenType LookupIdent(string ident)
    {
        return Keywords.GetValueOrDefault(ident, TokenType.Ident);
    }

    public override string ToString()
    {
        return $"{{Type: {TokenType}, Literal: {Literal}}}";
    }
}