namespace MonkeyLang.Evalutation;

public class Integer : IObject
{
    public long Value;

    public ObjectType Type()
    {
        return ObjectType.Integer;
    }

    public string Inspect()
    {
        return Value.ToString();
    }
}