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
}