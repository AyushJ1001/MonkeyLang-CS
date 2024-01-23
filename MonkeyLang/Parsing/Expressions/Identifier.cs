using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class Identifier : IExpression
{
    public Token Token;
    public string? Value;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        return Value ?? "";
    }
}