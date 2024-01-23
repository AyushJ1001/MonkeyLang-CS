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

    public static IObject Eval(INode? node)
    {
        switch (node)
        {
            // Statements
            case Parsing.Program program:
                return EvalProgram(program.Statements);
            case ExpressionStatement expressionStatement:
                return Eval(expressionStatement.Expression);
            // Expressions
            case IntegerLiteral literal:
                return new Integer { Value = literal.Value };
            case Parsing.Expressions.Boolean boolean:
                return boolean.Value ? TRUE : FALSE;
            case PrefixExpression prefixExpression:
                var right = Eval(prefixExpression.Right);
                if (IsError(right))
                {
                    return right;
                }
                return EvalPrefixExpression(prefixExpression.Operator, right);
            case InfixExpression infixExpression:
                var left = Eval(infixExpression.Left);
                if (IsError(left))
                {
                    return left;
                }

                right = Eval(infixExpression.Right);
                if (IsError(right))
                {
                    return right;
                }

                return EvalInfixExpression(infixExpression.Operator, left, right);
            case BlockStatement blockStatement:
                return EvalBlockStatement(blockStatement);
            case IfExpression ifExpression:
                var condition = Eval(ifExpression.Condition);
                if (IsError(condition))
                {
                    return condition;
                }
                return EvalIfExpression(ifExpression);
            case ReturnStatement returnStatement:
                var val = Eval(returnStatement.ReturnValue);
                if (IsError(val))
                {
                    return val;
                }
                return new ReturnValue { Value = val };
            default:
                return NULL;
        }
    }

    private static IObject EvalBlockStatement(BlockStatement blockStatement)
    {
        IObject? result = null;

        foreach (var statement in blockStatement.Statements)
        {
            result = Eval(statement);

            if (result != null)
            {
                var rt = result.Type();
                if (rt == ObjectType.ReturnValue || rt == ObjectType.Error)
                {
                    return result;
                }

                return result;
            }
        }

        return result ?? NULL;
    }

    private static IObject EvalIfExpression(IfExpression ifExpression)
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

    private static bool IsTruthy(IObject? obj)
    {
        return obj switch
        {
            var _ when obj == NULL => false,
            var boolean when boolean == TRUE => true,
            var boolean when boolean == FALSE => false,
            _ => true
        };
    }

    private static IObject EvalInfixExpression(string @operator, IObject? left, IObject? right)
    {
        if (left is Integer leftInteger && right is Integer rightInteger)
        {
            return EvalIntegerInfixExpression(@operator, leftInteger, rightInteger);
        }

        if (left?.Type() != right?.Type())
        {
            return NewError($"type mismatch: {left?.Type()} {@operator} {right?.Type()}");
        }

        return @operator switch
        {
            "==" => left == right ? TRUE : FALSE,
            "!=" => left != right ? TRUE : FALSE,
            _ => NewError($"unknown operator: {left?.Type()} {@operator} {right?.Type()}"),
        };
    }

    private static IObject EvalIntegerInfixExpression(string @operator, Integer leftInteger, Integer rightInteger)
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

    private static IObject EvalPrefixExpression(string @operator, IObject? right)
    {
        return @operator switch
        {
            "!" => EvalBangOperatorExpression(right),
            "-" => EvalMinusOperatorExpression(right),
            _ => NewError($"unknown operator: {@operator}{right?.Type()}"),
        };
    }

    private static IObject EvalMinusOperatorExpression(IObject? right)
    {
        if (right is not Integer integer)
        {
            return NewError($"unkown operator: -{right?.Type()}");
        }

        var value = integer.Value;
        return new Integer { Value = -value };
    }

    private static Boolean EvalBangOperatorExpression(IObject? right)
    {
        return right switch
        {
            var boolean when boolean == TRUE => FALSE,
            var boolean when boolean == FALSE => TRUE,
            var _ when right == NULL => TRUE,
            _ => FALSE,
        };
    }

    private static IObject EvalProgram(IEnumerable<IStatement> statements)
    {
        IObject result = NULL;

        foreach (var statement in statements)
        {
            result = Eval(statement);

            if (result is ReturnValue returnValue)
            {
                return returnValue.Value ?? NULL;
            }

            if (result is Error error)
            {
                return result;
            }
        }

        return result;
    }

    private static Error NewError(string message)
    {
        return new Error { Message = message };
    }

    private static bool IsError(IObject @object)
    {
        if (@object != null)
        {
            return @object.Type() == ObjectType.Error;
        }

        return false;
    }
}