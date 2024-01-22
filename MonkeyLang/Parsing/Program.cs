using System.Text;

namespace MonkeyLang.Parsing;

public class Program: INode
{
    public IList<IStatement> Statements = [];
    public string TokenLiteral()
    {
        return Statements.Count > 0 ? Statements[0].TokenLiteral() : "";
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var statement in Statements)
        {
            builder.Append(statement.ToString());
        }

        return builder.ToString();
    }
}