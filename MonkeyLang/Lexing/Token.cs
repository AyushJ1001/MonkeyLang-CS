namespace MonkeyLang.Lexing;

public enum TokenType
{
    Illegal,
    Eof,

    // Identifiers + literals
    Ident,
    Int,
    String,

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

public static class TokenTypeExtensions
{
    public static string TokenTypeString(this TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Illegal => "ILLEGAL",
            TokenType.Eof => "EOF",
            TokenType.Ident => "IDENT",
            TokenType.Int => "INT",
            TokenType.Assign => "=",
            TokenType.Plus => "+",
            TokenType.Minus => "-",
            TokenType.Bang => "!",
            TokenType.Asterisk => "*",
            TokenType.Slash => "/",
            TokenType.Lt => "<",
            TokenType.Gt => ">",
            TokenType.Eq => "==",
            TokenType.NotEq => "!=",
            TokenType.Comma => ",",
            TokenType.Semicolon => ";",
            TokenType.Lparen => "(",
            TokenType.Rparen => ")",
            TokenType.Lbrace => "{",
            TokenType.Rbrace => "}",
            TokenType.Function => "function",
            TokenType.Let => "let",
            TokenType.True => "true",
            TokenType.False => "false",
            TokenType.If => "if",
            TokenType.Else => "else",
            TokenType.Return => "return",
            TokenType.String => "String",
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null)
        };
    }
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