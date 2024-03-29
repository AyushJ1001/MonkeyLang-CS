using MonkeyLang;
using MonkeyLang.Evalutation;
using MonkeyLang.Lexing;
using MonkeyLang.Parsing;
using Array = MonkeyLang.Evalutation.Array;
using Boolean = MonkeyLang.Evalutation.Boolean;
using IObject = MonkeyLang.Evalutation.IObject;

namespace MonkeyTests;

public class EvaluatorTest
{
    [Fact]
    public void TestEvalIntegerExpression()
    {
        (string, long)[] tests =
        [
            ("5", 5),
            ("10", 10),
            ("-5", -5),
            ("-10", -10),
            ("5 + 5 + 5 + 5 - 10", 10),
            ("2 * 2 * 2 * 2 * 2", 32),
            ("-50 + 100 - 50", 0),
            ("5 * 2 + 10", 20),
            ("5 + 2 * 10", 25),
            ("20 + 2 * -10", 0),
            ("50 / 2 * 2 + 10", 60),
            ("2 * (5 + 10)", 30),
            ("3 * 3 * 3 + 10", 37),
            ("3 * (3 * 3) + 10", 37),
            ("(5 + 10 * 2 + 15 / 3) * 2 + -10", 50)
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);
            TestIntegerObject(evaluated, expected);
        }
    }

    [Fact]
    public void TestBooleanExpression()
    {
        (string, bool)[] tests =
        [
            ("true", true),
            ("false", false),
            ("1 < 2", true),
            ("1 > 2", false),
            ("1 < 1", false),
            ("1 > 1", false),
            ("1 == 1", true),
            ("1 != 1", false),
            ("1 == 2", false),
            ("1 != 2", true),
            ("true == true", true),
            ("false == false", true),
            ("true == false", false),
            ("true != false", true),
            ("false != true", true),
            ("(1 < 2) == true", true),
            ("(1 < 2) == false", false),
            ("(1 > 2) == true", false),
            ("(1 > 2) == false", true)
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);
            Assert.NotNull(evaluated);
            TestBooleanObject(evaluated, expected);
        }
    }

    [Fact]
    public void TestBangOperator()
    {
        (string, bool)[] tests =
        [
            ("!true", false),
            ("!false", true),
            ("!5", false),
            ("!!true", true),
            ("!!false", false),
            ("!!5", true)
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);
            Assert.NotNull(evaluated);
            TestBooleanObject(evaluated, expected);
        }
    }

    [Fact]
    public void TestIfElseExpressions()
    {
        (string, int?)[] tests =
        [
            ("if (true) { 10 }", 10),
            ("if (false) { 10 } ", null),
            ("if (1) { 10 }", 10),
            ("if (1 < 2) { 10 }", 10),
            ("if (1 > 2) { 10 }", null),
            ("if (1 > 2) { 10 } else { 20 }", 20),
            ("if (1 < 2) { 10 } else { 20 }", 10)
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);
            if (expected is int integer)
            {
                TestIntegerObject(evaluated, integer);
            }
            else
            {
                TestNullObject(evaluated);
            }
        }
    }

    [Fact]
    public void TestReturnStatements()
    {
        (string, long)[] tests =
        [
            ("return 10;", 10),
            ("return 10; 9;", 10),
            ("return 2 * 5; 9;", 10),
            ("9; return 2 * 5; 9;", 10)
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);
            TestIntegerObject(evaluated, expected);
        }
    }

    [Fact]
    public void TestErrorHandling()
    {
        (string, string)[] tests =
        [
            ("5 + true;", "type mismatch: Integer + Boolean"),
            ("5 + true; 5;", "type mismatch: Integer + Boolean"),
            ("-true", "unkown operator: -Boolean"),
            ("true + false;", "unknown operator: Boolean + Boolean"),
            ("5; true + false; 5", "unknown operator: Boolean + Boolean"),
            ("if (10 > 1) { true + false; }",
                "unknown operator: Boolean + Boolean"),
            ("if (10 > 1) { if (10 > 1) { return true + false; } return 1; }",
                "unknown operator: Boolean + Boolean"),
            ("foobar", "identifier not found: foobar"),
            (@"""Hello"" - ""World""", "unknown operator: String - String"),
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);

            Assert.IsType<Error>(evaluated);
            var error = (Error)evaluated;
            Assert.NotNull(evaluated);
            Assert.Equal(expected, error.Message);
        }
    }

    [Fact]
    public void TestLetStatements()
    {
        (string, long)[] tests =
        [
            ("let a = 5; a;", 5),
            ("let a = 5 * 5; a;", 25),
            ("let a = 5; let b = a; b;", 5),
            ("let a = 5; let b = a; let c = a + b + 5; c;", 15),
        ];

        foreach (var (input, expected) in tests)
        {
            TestIntegerObject(TestEval(input), expected);
        }
    }


    [Fact]
    public void TestFunctionObject()
    {
        const string input = "fn(x) { x + 2; };";

        var evaluated = TestEval(input);
        Assert.IsType<Function>(evaluated);
        var function = (Function)evaluated;

        Assert.Single(function.Parameters);
        Assert.Equal("x", function.Parameters[0].ToString());

        Assert.Equal("(x + 2)", function.Body.ToString());
    }

    [Fact]
    public void TestFunctionApplication()
    {
        (string, long)[] tests =
        [
            ("let identity = fn(x) {x;}; identity(5);", 5),
            ("let identity = fn(x) { return x;}; identity(5); ", 5),
            ("let double = fn(x) {x * 2;}; double(5);", 10),
            ("let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", 20),
            ("fn(x) {x;}(5)", 5),
        ];

        foreach (var (input, expected) in tests)
        {
            TestIntegerObject(TestEval(input), expected);
        }
    }

    [Fact]
    public void TestStringLiteral()
    {
        const string input = "\"Hello World!\"";

        var evaluated = TestEval(input);
        Assert.IsType<MonkeyLang.String>(evaluated);
        var str = (MonkeyLang.String)evaluated;

        Assert.Equal("Hello World!", str.Value);
    }

    [Fact]
    public void TestStringConcatenation()
    {
        const string input = @"""Hello"" + "" "" + ""World!""";

        var evaluated = TestEval(input);
        Assert.IsType<MonkeyLang.String>(evaluated);
        var str = (MonkeyLang.String)evaluated;

        Assert.Equal("Hello World!", str.Value);
    }

    [Fact]
    public void TestBuiltinFunctions()
    {
        (string, object)[] tests =
        [
            (@"len("""")", 0),
            (@"len(""four"")", 4),
            (@"len(""hello world"")", 11),
            ("len(1)", @"argument to ""len"" not supported, got Integer"),
            (@"len(""one"", ""two"")",
                @"wrong number of arguments. got=2, want=1")
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);

            switch (expected)
            {
                case int integer:
                    TestIntegerObject(evaluated, integer);
                    break;
                case string str:
                    Assert.IsType<Error>(evaluated);
                    var error = (Error)evaluated;

                    Assert.Equal(str, error.Message);
                    break;
            }
        }
    }

    [Fact]
    public void TestArrayLiterals()
    {
        const string input = "[1, 2 * 2, 3 + 3]";

        var evaluated = TestEval(input);
        Assert.IsType<Array>(evaluated);
        var result = (Array)evaluated;

        Assert.Equal(3, result.Elements.Count);

        TestIntegerObject(result.Elements[0], 1);
        TestIntegerObject(result.Elements[1], 4);
        TestIntegerObject(result.Elements[2], 6);
    }

    [Fact]
    public void TestArrayIndexExpressions()
    {
        (string, object)[] tests =
        [
            ("[1, 2, 3][0]", 1),
            ("[1, 2, 3][1]", 2),
            ("[1, 2, 3][2]", 3),
            ("let i = 0; [1][i];", 1),
            ("[1, 2, 3][1 + 1];", 3),
            ("let myArray = [1, 2, 3]; myArray[2];", 3),
            (
                "let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];",
                6
            ),
            (
                "let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]",
                2
            ),
            ("[1, 2, 3][3]", null),
            ("[1, 2, 3][-1]", null),
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);

            if (expected is int integer)
            {
                TestIntegerObject(evaluated, integer);
            }
            else
            {
                TestNullObject(evaluated);
            }
        }
    }

    private static void TestNullObject(IObject? @object)
    {
        Assert.Equal(Evaluator.NULL, @object);
    }

    private static void TestBooleanObject(IObject obj, bool expected)
    {
        Assert.IsType<Boolean>(obj);
        var result = (Boolean)obj;

        Assert.Equal(expected, result.Value);
    }

    private static void TestIntegerObject(IObject? obj, long expected)
    {
        Assert.IsType<Integer>(obj);
        var result = (Integer)obj;

        Assert.Equal(expected, result.Value);
    }

    private static IObject? TestEval(string input)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);
        var program = parser.ParseProgram();
        MonkeyLang.Environment env = new();

        Assert.NotNull(program);
        return Evaluator.Eval(program, env);
    }
}