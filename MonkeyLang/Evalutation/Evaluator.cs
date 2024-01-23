using System.Collections;
using System.Security.Cryptography;
using MonkeyLang.Parsing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Evalutation;

public class Evaluator
{
    public readonly static Boolean TRUE = new() { Value = true };
    public readonly static Boolean FALSE = new() { Value = false };
    public readonly static Null NULL = new();

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
            PrefixExpression prefixExpression => EvalPrefixExpression(prefixExpression.Operator, Eval(prefixExpression.Right)),
            InfixExpression infixExpression => EvalInfixExpression(infixExpression.Operator, Eval(infixExpression.Left), Eval(infixExpression.Right)),
            BlockStatement blockStatement => EvalStatements(blockStatement.Statements),
            IfExpression ifExpression => EvalIfExpression(ifExpression),
            _ => NULL
        };
    }

    private static Object? EvalIfExpression(IfExpression ifExpression)
    {
        var condition = Eval(ifExpression.Condition);
        if (IsTruthy(condition))
        {
            return Eval(ifExpression.Consequence);
        }
        if (ifExpression.Alternative is not null)
        {
            return Eval(ifExpression.Alternative);
        }
        return NULL;
    }

    private static bool IsTruthy(Object? obj)
    {
        return obj switch
        {
            var _ when obj == NULL => false,
            var boolean when boolean == TRUE => true,
            var boolean when boolean == FALSE => false,
            _ => true
        };
    }

    private static Object EvalInfixExpression(string @operator, Object? left, Object? right)
    {
        if (left is Integer leftInteger && right is Integer rightInteger)
        {
            return EvalIntegerInfixExpression(@operator, leftInteger, rightInteger);
        }

        return @operator switch
        {
            "==" => left == right ? TRUE : FALSE,
            "!=" => left != right ? TRUE : FALSE,
            _ => NULL,
        };
    }

    private static Object EvalIntegerInfixExpression(string @operator, Integer leftInteger, Integer rightInteger)
    {
        return @operator switch
        {
            "+" => new Integer { Value = leftInteger.Value + rightInteger.Value },
            "-" => new Integer { Value = leftInteger.Value - rightInteger.Value },
            "*" => new Integer { Value = leftInteger.Value * rightInteger.Value },
            "/" => new Integer { Value = leftInteger.Value / rightInteger.Value },
            "<" => leftInteger.Value < rightInteger.Value ? TRUE : FALSE,
            ">" => leftInteger.Value > rightInteger.Value ? TRUE : FALSE,
            "==" => leftInteger.Value == rightInteger.Value ? TRUE : FALSE,
            "!=" => leftInteger.Value != rightInteger.Value ? TRUE : FALSE,
            _ => NULL
        };
    }

    private static Object EvalPrefixExpression(string @operator, Object? right)
    {
        return @operator switch
        {
            "!" => EvalBangOperatorExpression(right),
            "-" => EvalMinusOperatorExpression(right),
            _ => NULL,
        };
    }

    private static Object EvalMinusOperatorExpression(Object? right)
    {
        if (right is not Integer integer)
        {
            return NULL;
        }

        var value = integer.Value;
        return new Integer { Value = -value };
    }

    private static Boolean EvalBangOperatorExpression(Object? right)
    {
        return right switch
        {
            var boolean when boolean == TRUE => FALSE,
            var boolean when boolean == FALSE => TRUE,
            var _ when right == NULL => TRUE,
            _ => FALSE,
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