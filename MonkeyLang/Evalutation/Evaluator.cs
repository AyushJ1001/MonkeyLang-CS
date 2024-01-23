using MonkeyLang.Parsing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Evalutation;

public class Evaluator
{
    public static Boolean TRUE = new Boolean { Value = true };
    public static Boolean FALSE = new Boolean { Value = false };
    public static Null NULL = new Null();

    public static Object? Eval(INode? node)
    {
        return node switch
        {
            // Statements
            Parsing.Program program => EvalStatements(program.Statements),
            ExpressionStatement expressionStatement => Eval(expressionStatement
                .Expression),
            // Expressions
            IntegerLiteral literal => new Integer { Value = literal.Value },
            Parsing.Expressions.Boolean boolean => boolean.Value ? TRUE : FALSE,
            _ => NULL
        };
    }

    private static Object? EvalStatements(IEnumerable<IStatement> statements)
    {
        Object? result = null;

        foreach (var statement in statements)
        {
            result = Eval(statement);
        }

        return result;
    }
}