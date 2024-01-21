namespace MonkeyLang.Parsing;

public class Program: INode
{
    public IList<IStatement> Statements = [];
    public string TokenLiteral()
    {
        return Statements.Count > 0 ? Statements[0].TokenLiteral() : "";
    }
}