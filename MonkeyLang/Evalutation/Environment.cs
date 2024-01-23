using MonkeyLang.Evalutation;

namespace MonkeyLang;

public class Environment
{
    private Dictionary<string, IObject> Store;
    private Environment? Outer;

    public Environment()
    {
        Store = [];
        Outer = null;
    }

    public IObject? Get(string name)
    {
        if (Store.TryGetValue(name, out var obj))
        {
            return obj;
        }

        return Outer?.Get(name);
    }

    public IObject Set(string name, IObject value)
    {
        Store[name] = value;
        return value;
    }

    public Environment NewEnclosedEnvironment()
    {
        return new Environment
        {
            Outer = this
        };
    }
}
