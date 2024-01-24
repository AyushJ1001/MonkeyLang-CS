namespace MonkeyLang.Evalutation;

public class Array: IObject
{
    public IList<IObject> Elements;
    public ObjectType Type()
    {
        return ObjectType.Array;
    }

    public string Inspect()
    {
        var elements = Elements.Select(e => e.Inspect());
        return $"[{string.Join(", ", elements)}]";
    }
}