using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class IndexExpression: IExpression
{
    public Token Token;
    public IExpression? Left;
    public IExpression? Index;
    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        return $"({Left}[{Index}])";
    }
}