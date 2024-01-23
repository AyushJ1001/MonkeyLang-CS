namespace MonkeyLang.Evalutation;

public enum ObjectType
{
    Integer,
    Boolean,
    Null,
    ReturnValue
}

public interface IObject
{
    ObjectType Type();
    string Inspect();
}