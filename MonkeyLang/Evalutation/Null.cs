namespace MonkeyLang.Evalutation;

public class Null : IObject
{
    public ObjectType Type()
    {
        return ObjectType.Null;
    }

    public string Inspect()
    {
        return "null";
    }
}