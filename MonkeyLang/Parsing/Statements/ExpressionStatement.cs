using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Statements;

public class ExpressionStatement(Token token) : IStatement
{
    public Token Token = token;
    public IExpression? Expression;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void StatementNode()
    {
    }

    public override string ToString()
    {
        return Expression?.ToString() ?? "";
    }
}