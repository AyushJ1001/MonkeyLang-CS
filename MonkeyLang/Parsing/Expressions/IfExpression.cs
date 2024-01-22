using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Parsing.Expressions;

public class IfExpression : IExpression
{
    public Token Token;
    public IExpression? Condition;
    public BlockStatement? Consequence;
    public BlockStatement? Alternative;

    public string TokenLiteral()
    {
        return Token.Literal;
    }

    public void ExpressionNode()
    {
    }

    public override string ToString()
    {
        var alternative = Alternative != null ? $"else {Alternative}" : "";
        return $"if {Condition} {Consequence} {alternative}";
    }
}