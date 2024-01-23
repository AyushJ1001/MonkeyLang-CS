using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class Boolean: IExpression
{
    public Token Token;
    public bool Value;
    public string TokenLiteral()
    {
        return Token.Literal.ToLower();
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        return Token.Literal.ToLower();
    }
}