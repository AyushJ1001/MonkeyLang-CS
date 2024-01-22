using System.Text;
using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Parsing.Expressions;

public class FunctionLiteral: IExpression
{
    public Token Token;
    public IList<Identifier>? Parameters;
    public BlockStatement? Body;
    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        var paramsList = Parameters.Select(p => p.ToString());
        return $"{TokenLiteral()}({string.Join(", ", paramsList)}) {Body}";
    }
}