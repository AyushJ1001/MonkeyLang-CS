using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class CallExpression: IExpression
{
    public Token Token;
    public IExpression? Function;
    public IList<IExpression?>? Arguments;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        var args = Arguments.Select(arg => arg.ToString());

        return $"{Function}({string.Join(", ", args)})";
    }
}