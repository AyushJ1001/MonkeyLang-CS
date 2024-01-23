using MonkeyLang.Evalutation;

namespace MonkeyLang;

// function that takes a variable number of IObject and returns an IObject
using BuiltinFunction = Func<IObject[], IObject>;

public class Builtin : IObject
{
    public static Dictionary<string, Builtin> builtins = new() {
        {"len", new Builtin()
            {
                Fn = (args) => {
                    if (args.Length != 1) {
                        return Evaluator.NewError($"wrong number of arguments. got={args.Length}, want=1");
                    }

                    return args[0] switch {
                        String str => new Integer { Value = str.Value.Length},
                        _ => Evaluator.NewError(@$"argument to ""len"" not supported, got {args[0].Type()}"),
                    };
                }
            }
        },
    };
    public required BuiltinFunction Fn;

    public string Inspect()
    {
        return "builtin function";
    }

    public ObjectType Type()
    {
        return ObjectType.Builtin;
    }
}
