using MonkeyLang.Evalutation;

namespace MonkeyLang;

public struct Environment
{
    private readonly Dictionary<string, IObject> Store;

    public Environment()
    {
        Store = [];
    }

    public readonly IObject? Get(string name)
    {
        return Store.TryGetValue(name, out IObject? value) ? value : null;
    }

    public readonly IObject Set(string name, IObject value)
    {
        Store[name] = value;
        return value;
    }
}
