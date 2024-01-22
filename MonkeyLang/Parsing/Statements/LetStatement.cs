using System.Text;
using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Expressions;
namespace MonkeyLang.Parsing.Statements;

public class LetStatement :
    IStatement
{
    public Token Token;
    public Identifier? Name;
    public IExpression? Value;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void StatementNode() { }

    public override string ToString()
    {
        StringBuilder builder = new();

        builder.Append(TokenLiteral() + " ");
        builder.Append(Name?.ToString() ?? "");
        builder.Append(" = ");
        builder.Append(Value?.ToString() ?? "");
        builder.Append(';');

        return builder.ToString();
    }
}