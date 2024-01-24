using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class ArrayLiteral: IExpression
{
    public required Token Token;
    public IList<IExpression>? Elements;
    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode() { }

    public override string ToString()
    {
        var elements = Elements.Select(el => el.ToString());
        return $"[{string.Join(", ", elements)}]";
    }
}