using MonkeyLang.Evalutation;

namespace MonkeyLang;

public class ReturnValue : IObject
{
    public IObject? Value { get; set; }

    public string Inspect()
    {
        return Value?.Inspect() ?? "";
    }

    public ObjectType Type()
    {
        return ObjectType.ReturnValue;
    }
}
