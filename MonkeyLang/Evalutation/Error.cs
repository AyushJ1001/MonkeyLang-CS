using MonkeyLang.Evalutation;

namespace MonkeyLang;

public class Error : IObject
{
    public string Message = "";

    public string Inspect()
    {
        return $"Error: {Message}";
    }

    public ObjectType Type()
    {
        return ObjectType.Error;
    }
}
