using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class IntegerLiteral: IExpression
{
    public Token Token;
    public long Value;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        return Token.Literal;
    }
}