namespace MonkeyLang.Evalutation;

public enum ObjectType
{
    Integer,
    Boolean,
    Null,
    ReturnValue,
    Error,
    Function
}

public interface IObject
{
    ObjectType Type();
    string Inspect();
}