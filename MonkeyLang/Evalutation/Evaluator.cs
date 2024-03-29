using System.Collections;
using System.Security.Cryptography;
using MonkeyLang.Parsing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Evalutation;

using System.Collections.Generic;

public static class IEnumerableExtensions
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
       => self.Select((item, index) => (item, index));
}

public class Evaluator
{
    public readonly static Boolean TRUE = new() { Value = true };
    public readonly static Boolean FALSE = new() { Value = false };
    public readonly static Null NULL = new();

    public static IObject Eval(INode? node, Environment env)
    {
        switch (node)
        {
            // Statements
            case Parsing.Program program:
                return EvalProgram(program.Statements, env);
            case ExpressionStatement expressionStatement:
                return Eval(expressionStatement.Expression, env);
            // Expressions
            case IntegerLiteral literal:
                return new Integer { Value = literal.Value };
            case Parsing.Expressions.Boolean boolean:
                return boolean.Value ? TRUE : FALSE;
            case PrefixExpression prefixExpression:
                var right = Eval(prefixExpression.Right, env);
                if (IsError(right))
                {
                    return right;
                }
                return EvalPrefixExpression(prefixExpression.Operator, right);
            case InfixExpression infixExpression:
                var left = Eval(infixExpression.Left, env);
                if (IsError(left))
                {
                    return left;
                }

                right = Eval(infixExpression.Right, env);
                if (IsError(right))
                {
                    return right;
                }

                return EvalInfixExpression(infixExpression.Operator, left, right);
            case BlockStatement blockStatement:
                return EvalBlockStatement(blockStatement, env);
            case IfExpression ifExpression:
                var condition = Eval(ifExpression.Condition, env);
                return IsError(condition) ? condition : EvalIfExpression(ifExpression, env);
            case ReturnStatement returnStatement:
                var val = Eval(returnStatement.ReturnValue, env);
                return IsError(val) ? val : new ReturnValue { Value = val };
            case LetStatement letStatement:
                val = Eval(letStatement.Value, env);
                return IsError(val) ? val : env.Set(letStatement.Name?.Value ?? "", val);
            case Identifier identifier:
                return EvalIdentifier(identifier, env);
            case FunctionLiteral functionLiteral:
                var parameters = functionLiteral.Parameters ?? [];
                var body = functionLiteral.Body ?? new();
                return new Function { Parameters = parameters, Body = body, Env = env };
            case CallExpression call:
                var function = Eval(call.Function, env);
                if (IsError(function))
                {
                    return function;
                }
                var args = EvalExpressions(call.Arguments, env);
                if (args.Count == 1 && IsError(args[0]))
                {
                    return args[0];
                }

                return ApplyFunction(function, args);
            case StringLiteral stringLiteral:
                return new String { Value = stringLiteral.Value };
            case ArrayLiteral arrayLiteral:
                var elements = EvalExpressions(arrayLiteral.Elements, env);
                if (elements.Count == 1 && IsError(elements[0]))
                {
                    return elements[0];
                }
                return new Array { Elements = elements };
            case IndexExpression indexExpression:
                left = Eval(indexExpression.Left, env);
                if (IsError(left))
                {
                    return left;
                }

                var index = Eval(indexExpression.Index, env);
                if (IsError(index))
                {
                    return index;
                }

                return EvalIndexExpression(left, index);
            default:
                return NULL;
        }
    }

    private static IObject EvalIndexExpression(IObject left, IObject index)
    {
        if (left.Type() == ObjectType.Array &&
            index.Type() == ObjectType.Integer)
        {
            return EvalArrayIndexExpression(left, index);
        }

        return NewError($"index operator not supported: {left.Type()}");
    }

    private static IObject EvalArrayIndexExpression(IObject array, IObject
            index)
    {
        var arrayObject = (Array)array;
        var idx = ((Integer)index).Value;
        var max = (long)(arrayObject.Elements.Count - 1);

        if (idx < 0 || idx > max)
        {
            return NULL;
        }

        return arrayObject.Elements[(int)idx];
    }

    private static IObject ApplyFunction(IObject function, List<IObject> args)
    {

        switch (function)
        {
            case Function fn:
                var extendedEnv = ExtendFunctionEnv(fn, args);
                var evaluated = Eval(fn.Body, extendedEnv);
                return UnwrapReturnValue(evaluated);
            case Builtin builtin:
                return builtin.Fn([.. args]);
            default:
                return NewError($"not a function: {function.Type()}");

        }
    }

    private static IObject UnwrapReturnValue(IObject obj)
    {
        if (obj is ReturnValue returnValue)
        {
            return returnValue.Value ?? obj;
        }

        return obj;
    }

    private static Environment ExtendFunctionEnv(Function fn, List<IObject> args)
    {
        var env = fn.Env.NewEnclosedEnvironment();
        foreach (var (param, paramIdx) in fn.Parameters.WithIndex())
        {
            if (param.Value is null) continue;
            if (paramIdx >= args.Count)
            {
                continue;
            }
            _ = env.Set(param.Value, args[paramIdx]);
        }

        return env;
    }

    private static List<IObject> EvalExpressions(IList<IExpression>? expressions, Environment env)
    {
        var result = new List<IObject>();
        expressions ??= [];

        foreach (var exp in expressions)
        {
            var evaluated = Eval(exp, env);
            if (IsError(evaluated))
            {
                return [evaluated];
            }
            result.Add(evaluated);
        }

        return result;
    }

    private static IObject EvalIdentifier(Identifier identifier, Environment env)
    {
        if (env.Get(identifier.Value ?? "") is IObject val)
        {
            return val;
        }

        if (Builtin.builtins.TryGetValue(identifier.Value ?? "", out var builtin))
        {
            return builtin;
        }

        return NewError($"identifier not found: {identifier.Value}");
    }

    private static IObject EvalBlockStatement(BlockStatement blockStatement, Environment env)
    {
        IObject? result = null;

        foreach (var statement in blockStatement.Statements)
        {
            result = Eval(statement, env);

            if (result is not (ReturnValue or Error)) continue;
            var rt = result.Type();
            return result;
        }

        return result ?? NULL;
    }

    private static IObject EvalIfExpression(IfExpression ifExpression, Environment env)
    {
        var condition = Eval(ifExpression.Condition, env);
        if (IsTruthy(condition))
        {
            return Eval(ifExpression.Consequence, env);
        }
        if (ifExpression.Alternative is not null)
        {
            return Eval(ifExpression.Alternative, env);
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

        if (left?.Type() == ObjectType.String)
        {
            return EvalStringInfixExpression(@operator, left, right);
        }

        return @operator switch
        {
            "==" => left == right ? TRUE : FALSE,
            "!=" => left != right ? TRUE : FALSE,
            _ => NewError($"unknown operator: {left?.Type()} {@operator} {right?.Type()}"),
        };
    }

    private static IObject EvalStringInfixExpression(string @operator, IObject left, IObject right)
    {
        if (@operator != "+")
        {
            return NewError($"unknown operator: {left.Type()} {@operator} {right?.Type()}");
        }

        var leftVal = ((String)left).Value;
        var rightVal = ((String)right).Value;
        return new String { Value = leftVal + rightVal };
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

    private static IObject EvalProgram(IEnumerable<IStatement?> statements, Environment env)
    {
        IObject result = NULL;

        foreach (var statement in statements)
        {
            result = Eval(statement, env);

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

    public static Error NewError(string message)
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