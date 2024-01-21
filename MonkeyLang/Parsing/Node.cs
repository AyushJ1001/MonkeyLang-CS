namespace MonkeyLang.Parsing;

public interface INode
{
    string TokenLiteral();
}

public interface IStatement : INode
{
    void StatementNode();
}

public interface IExpression : INode
{
    void ExpressionNode();
}