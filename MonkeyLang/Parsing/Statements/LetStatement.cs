using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Expressions;
namespace MonkeyLang.Parsing.Statements;

public class LetStatement(Token token) : IStatement
{
    public Token Token = token;
    public Identifier? Name;
    public IExpression? Value;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void StatementNode() { }
}