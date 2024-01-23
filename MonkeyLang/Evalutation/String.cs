using MonkeyLang.Evalutation;

namespace MonkeyLang;

public class String : IObject
{
    public string Value = "";

    public string Inspect()
    {
        return Value;
    }

    public ObjectType Type()
    {
        return ObjectType.String;
    }
}
