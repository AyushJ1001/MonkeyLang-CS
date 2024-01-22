using MonkeyLang.Lexing;

namespace MonkeyLang.Parsing.Statements;

public class BlockStatement: IStatement
{
    public Token Token;
    public IList<IStatement> Statements;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void StatementNode()
    {
    }

    public override string ToString()
    {
        return string.Join(" ", Statements);
    }
}