using MonkeyLang.Evalutation;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang;

public class Function : IObject
{
    public IList<Identifier> Parameters = [];
    public BlockStatement Body = new();
    public Environment Env = new();

    public string Inspect()
    {
        var parameters = Parameters.Select(p => p.ToString());
        return $"fn({string.Join(", ", parameters)}) {{\n{Body}\n}}";
    }

    public ObjectType Type()
    {
        return ObjectType.Function;
    }
}
