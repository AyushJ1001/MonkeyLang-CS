namespace MonkeyLang.Evalutation;

public enum ObjectType
{
    Integer,
    Boolean,
    Null
}

public interface Object
{
    ObjectType Type();
    string Inspect();
}