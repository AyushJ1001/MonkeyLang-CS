using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Expressions;

public class Identifier(Token token, string value) : IExpression
{
    public Token Token = token;
    public string Value = value;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
        throw new NotImplementedException();
    }
}