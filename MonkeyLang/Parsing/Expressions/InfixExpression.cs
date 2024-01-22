using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class InfixExpression: IExpression
{
    public Token Token;
    public IExpression? Left;
    public string Operator;
    public IExpression? Right;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        return $"({Left} {Operator} {Right})";
    }
}