namespace MonkeyLang.Evalutation;

public class Boolean : IObject
{
    public ObjectType Type()
    {
        return ObjectType.Boolean;
    }

    public string Inspect()
    {
        return Value.ToString().ToLower();
    }

    public bool Value { get; set; }
}