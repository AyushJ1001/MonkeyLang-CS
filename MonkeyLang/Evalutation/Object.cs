namespace MonkeyLang.Evalutation;

public enum ObjectType
{
    Integer,
    Boolean,
    Null,
    ReturnValue,
    Error,
    Function,
    String,
    Builtin,
    Array
}

public interface IObject
{
    ObjectType Type();
    string Inspect();
}