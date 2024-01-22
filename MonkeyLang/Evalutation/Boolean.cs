namespace MonkeyLang.Evalutation;

public class Boolean: Object
{
    public ObjectType Type()
    {
        return ObjectType.Boolean;
    }

    public string Inspect()
    {
        return Value.ToString();
    }

    public bool Value { get; set; }
}