using System.Text;
using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Statements;

public class ReturnStatement(Token token) : IStatement
{
    public Token Token = token;
    public IExpression? ReturnValue;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void StatementNode()
    {
        
    }

    public override string ToString()
    {
        StringBuilder builder = new();

        builder.Append(TokenLiteral() + " ");
        builder.Append(ReturnValue?.ToString() ?? "");
        builder.Append(';');

        return builder.ToString();
    }
}