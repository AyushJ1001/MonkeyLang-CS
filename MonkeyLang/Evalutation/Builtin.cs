using MonkeyLang.Evalutation;
using Array = MonkeyLang.Evalutation.Array;

namespace MonkeyLang;

// function that takes a variable number of IObject and returns an IObject
using BuiltinFunction = Func<IObject[], IObject>;

public class Builtin : IObject
{
    public static Dictionary<string, Builtin> builtins = new()
    {
        {
            "len", new Builtin()
            {
                Fn = (args) =>
                {
                    if (args.Length != 1)
                    {
                        return Evaluator.NewError(
                            $"wrong number of arguments. got={args.Length}, want=1");
                    }

                    return args[0] switch
                    {
                        Array array => new Integer
                            { Value = array.Elements.Count },
                        String str => new Integer { Value = str.Value.Length },
                        _ => Evaluator.NewError(
                            @$"argument to ""len"" not supported, got {args[0].Type()}"),
                    };
                }
            }
        },
        {
            "first", new Builtin()
            {
                Fn = (args) =>
                {
                    if (args.Length != 1)
                    {
                        return Evaluator.NewError(
                            $"wrong number of arguments. got={args.Length}, want=1");
                    }

                    if (args[0] is not Array array)
                    {
                        return Evaluator.NewError(
                            $"""argument to "first" must be ARRAY, got {args[0].Type()}""");
                    }

                    if (array.Elements.Count > 0)
                    {
                        return array.Elements[0];
                    }

                    return Evaluator.NULL;
                }
            }
        },
        {"last", new Builtin()
        {
            Fn = (args) =>
            {
                if (args.Length != 1)
                {
                    return Evaluator.NewError(
                        $"wrong number of arguments. got={args.Length}, want=1");
                }

                if (args[0] is not Array array)
                {
                    return Evaluator.NewError(
                        $"""argument to "last" must be ARRAY, got {args[0].Type()}""");
                }

                if (array.Elements.Count > 0)
                {
                    return array.Elements[^1];
                }

                return Evaluator.NULL;
            }
        }
        },
        {"rest", new Builtin()
        {
            Fn = (args) =>
            {
                if (args.Length != 1)
                {
                    return Evaluator.NewError(
                        $"wrong number of arguments. got={args.Length}, want=1");
                }

                if (args[0] is not Array array)
                {
                    return Evaluator.NewError(
                        $"""argument to "rest" must be ARRAY, got {args[0].Type()}""");
                }

                if (array.Elements.Count > 0)
                {
                    var newElements = new List<IObject>(array.Elements);
                    return new Array { Elements = newElements[1..] };
                }

                return Evaluator.NULL;
            }
        }},
        {"push", new Builtin()
        {
            Fn = (args) =>
            {
                if (args.Length != 2)
                {
                    return Evaluator.NewError(
                        $"wrong number of arguments. got={args.Length}, want=2");
                }

                if (args[0] is not Array array)
                {
                    return Evaluator.NewError($"""argument to "push" must be ARRAY, got {args[0].Type()}""");
                }

                var newElements = new List<IObject>(array.Elements);
                newElements.Add(args[1]);
                return new Array { Elements = newElements };
            }
        }}
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