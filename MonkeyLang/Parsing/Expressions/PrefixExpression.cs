using System.Text;
using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class PrefixExpression: IExpression
{
    public Token Token;
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
        return $"({Operator}{Right})";
    }
}