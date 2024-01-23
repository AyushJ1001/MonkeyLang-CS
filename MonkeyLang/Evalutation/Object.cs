namespace MonkeyLang.Evalutation;

public enum ObjectType
{
    Integer,
    Boolean,
    Null,
    ReturnValue,
    Error
}

public interface IObject
{
    ObjectType Type();
    string Inspect();
}