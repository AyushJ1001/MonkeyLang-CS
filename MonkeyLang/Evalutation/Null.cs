namespace MonkeyLang.Evalutation;

public class Null: Object
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