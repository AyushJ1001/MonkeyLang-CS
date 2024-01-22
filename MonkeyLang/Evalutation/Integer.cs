namespace MonkeyLang.Evalutation;

public class Integer: Object
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