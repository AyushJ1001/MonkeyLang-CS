using MonkeyLang.Lexing;
using MonkeyLang.Parsing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;
using Xunit.Abstractions;
using Xunit.Sdk;
using Boolean = MonkeyLang.Parsing.Expressions.Boolean;

namespace MonkeyTests;

public class ParserTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestLetStatements()
    {
        (string, string, object)[] tests =
        [
            ("let x = 5;", "x", 5),
            ("let y = true;", "y", true),
            ("let foobar = y;", "foobar", "y")
        ];

        foreach (var (input, ident, value) in tests)
        {
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            Assert.NotNull(program);
            Assert.Single(program.Statements);

            Assert.True(TestLetStatement(program.Statements[0], ident));

            var letStatement = (LetStatement)program.Statements[0];

            TestLiteralExpression(letStatement.Value, value);
        }
    }

    [Fact]
    public void TestReturnStatements()
    {
        var input = """
                    return 5;
                    return 10;
                    return 993322;
                    """;

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);

        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        if (program == null)
        {
            throw new Exception("Program is null");
        }

        if (program.Statements.Count != 3)
        {
            throw new Exception(
                $"program.Statements does not contain 3 statements. got={program.Statements.Count}");
        }

        foreach (var statement in program.Statements)
        {
            if (statement is not ReturnStatement returnStatement)
            {
                throw new Exception(
                    $"statement not ReturnStatement. got={statement.GetType()}");
            }

            if (returnStatement.TokenLiteral() != "return")
            {
                throw new Exception(
                    $"returnStmt.TokenLiteral not 'return', got {returnStatement.TokenLiteral()}.");
            }
        }
    }

    [Fact]
    public void TestIdentifierExpression()
    {
        const string input = "foobar;";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);

        Assert.Equal(1, program.Statements.Count);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<Identifier>(statement.Expression);
        var ident = (Identifier)statement.Expression;

        Assert.Equal("foobar", ident.Value);
        Assert.Equal("foobar", ident.TokenLiteral());
    }

    [Fact]
    public void TestIntegerLiteralExpression()
    {
        const string input = "5;";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);
        Assert.Equal(1, program.Statements.Count);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<IntegerLiteral>(statement.Expression);
        var literal = (IntegerLiteral)statement.Expression;

        Assert.Equal(5, literal.Value);
    }

    [Fact]
    public void TestParsingPrefixExpressions()
    {
        var prefixTests = new (string, string, object)[]
        {
            ("!5;", "!", 5),
            ("-15;", "-", 15),
            ("!false;", "!", false),
            ("!true;", "!", true)
        };

        foreach (var (input, op, value) in prefixTests)
        {
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            Assert.NotNull(program);
            Assert.Equal(1, program.Statements.Count);

            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var statement = (ExpressionStatement)program.Statements[0];

            Assert.IsType<PrefixExpression>(statement.Expression);
            var expression = (PrefixExpression)statement.Expression;

            Assert.Equal(op, expression.Operator);
            TestLiteralExpression(expression.Right, value);
        }
    }

    [Fact]
    public void TestParsingInfixExpressions()
    {
        var infixTests = new (string, object, string, object)[]
        {
            ("5 + 5;", 5, "+", 5),
            ("5 - 5;", 5, "-", 5),
            ("5 * 5;", 5, "*", 5),
            ("5 / 5;", 5, "/", 5),
            ("5 > 5;", 5, ">", 5),
            ("5 < 5;", 5, "<", 5),
            ("5 == 5;", 5, "==", 5),
            ("5 != 5;", 5, "!=", 5),
            ("true == true", true, "==", true),
            ("true != false", true, "!=", false),
            ("false == false", false, "==", false)
        };

        foreach (var (input, left, op, right) in infixTests)
        {
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            Assert.NotNull(program);
            Assert.Equal(1, program.Statements.Count);

            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var statement = (ExpressionStatement)program.Statements[0];

            Assert.IsType<InfixExpression>(statement.Expression);
            var expression = (InfixExpression)statement.Expression;

            TestLiteralExpression(expression.Left, left);
            TestLiteralExpression(expression.Right, right);
        }
    }

    [Fact]
    public void TestBooleanExpression()
    {
        const string input = "true;";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);
        Assert.Equal(1, program.Statements.Count);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<Boolean>(statement.Expression);
        var expression = (Boolean)statement.Expression;

        Assert.True(expression.Value);
    }

    [Fact]
    public void TestOperatorPrecedenceParsing()
    {
        var tests = new[]
        {
            (
                "-a * b",
                "((-a) * b)"
            ),
            (
                "!-a",
                "(!(-a))"
            ),
            (
                "a + b + c",
                "((a + b) + c)"
            ),
            (
                "a + b - c",
                "((a + b) - c)"
            ),
            (
                "a * b * c",
                "((a * b) * c)"
            ),
            (
                "a * b / c",
                "((a * b) / c)"
            ),
            (
                "a + b / c",
                "(a + (b / c))"
            ),
            (
                "a + b * c + d / e - f",
                "(((a + (b * c)) + (d / e)) - f)"
            ),
            (
                "3 + 4; -5 * 5",
                "(3 + 4)((-5) * 5)"
            ),
            (
                "5 > 4 == 3 < 4",
                "((5 > 4) == (3 < 4))"
            ),
            (
                "5 < 4 != 3 > 4",
                "((5 < 4) != (3 > 4))"
            ),
            (
                "3 + 4 * 5 == 3 * 1 + 4 * 5",
                "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"
            ),
            (
                "3 + 4 * 5 == 3 * 1 + 4 * 5",
                "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"
            ),
            ("true", "true"),
            ("false", "false"),
            ("3 > 5 == false", "((3 > 5) == false)"),
            ("3 < 5 == true", "((3 < 5) == true)"),
            ("1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)"),
            ("(5 + 5) * 2", "((5 + 5) * 2)"),
            ("2 / (5 + 5)", "(2 / (5 + 5))"),
            ("-(5 + 5)", "(-(5 + 5))"),
            ("!(true == true)", "(!(true == true))")
        };

        foreach (var (input, expected) in tests)
        {
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            Assert.NotNull(program);
            Assert.Equal(expected, program.ToString());
        }
    }

    [Fact]
    public void TestIfExpression()
    {
        const string input = "if (x < y) { x }";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);
        Assert.Equal(1, program.Statements.Count);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<IfExpression>(statement.Expression);
        var expression = (IfExpression)statement.Expression;

        TestInfixExpression(expression.Condition, "x", "<", "y");

        Assert.NotNull(expression.Consequence);
        Assert.Equal(1, expression.Consequence.Statements.Count);

        Assert.IsType<ExpressionStatement>(expression.Consequence
            .Statements[0]);
        var consequence =
            (ExpressionStatement)expression.Consequence.Statements[0];

        TestIdentifier(consequence.Expression, "x");

        Assert.Null(expression.Alternative);
    }

    private static void TestIntegerLiteral(IExpression? expression, int value)
    {
        Assert.IsType<IntegerLiteral>(expression);
        var integerLiteral = (IntegerLiteral)expression;

        Assert.Equal(value, integerLiteral.Value);
        Assert.Equal(value.ToString(), integerLiteral.TokenLiteral());
    }

    [Fact]
    public void TestIfElseExpression()
    {
        const string input = "if (x < y) { x } else { y }";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);
        Assert.Equal(1, program.Statements.Count);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<IfExpression>(statement.Expression);
        var expression = (IfExpression)statement.Expression;

        TestInfixExpression(expression.Condition, "x", "<", "y");

        Assert.NotNull(expression.Consequence);
        Assert.Equal(1, expression.Consequence.Statements.Count);

        Assert.IsType<ExpressionStatement>(expression.Consequence
            .Statements[0]);
        var consequence =
            (ExpressionStatement)expression.Consequence.Statements[0];

        TestIdentifier(consequence.Expression, "x");

        Assert.NotNull(expression.Alternative);
        Assert.Equal(1, expression.Alternative.Statements.Count);

        Assert.IsType<ExpressionStatement>(expression.Alternative
            .Statements[0]);
        var alternative =
            (ExpressionStatement)expression.Alternative.Statements[0];

        TestIdentifier(alternative.Expression, "y");
    }

    [Fact]
    public void TestFunctionalLiteralParsing()
    {
        const string input = "fn(x, y) { x + y; }";

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);
        Assert.Equal(1, program.Statements.Count);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<FunctionLiteral>(statement.Expression);
        var function = (FunctionLiteral)statement.Expression;

        Assert.NotNull(function.Parameters);
        Assert.Equal(2, function.Parameters.Count);

        TestLiteralExpression(function.Parameters[0], "x");
        TestLiteralExpression(function.Parameters[1], "y");

        Assert.NotNull(function.Body);
        Assert.Equal(1, function.Body.Statements.Count);
        Assert.IsType<ExpressionStatement>(function.Body.Statements[0]);
        var bodyStatement = (ExpressionStatement)function.Body.Statements[0];

        TestInfixExpression(bodyStatement.Expression, "x", "+", "y");
    }

    [Fact]
    public void TestFunctionParameterParsing()
    {
        (string, string[])[] tests =
        [
            ("fn() {};", []),
            ("fn(x) {};", ["x"]),
            ("fn(x, y, z) {};", ["x", "y", "z"])
        ];

        foreach (var (input, expectedParams) in tests)
        {
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            CheckParserErrors(parser);

            Assert.NotNull(program);
            Assert.IsType<ExpressionStatement>(program.Statements[0]);
            var statement = (ExpressionStatement)program.Statements[0];

            Assert.IsType<FunctionLiteral>(statement.Expression);
            var function = (FunctionLiteral)statement.Expression;

            Assert.NotNull(function.Parameters);
            Assert.Equal(expectedParams.Length, function.Parameters.Count);

            for (var i = 0; i < expectedParams.Length; i++)
            {
                TestLiteralExpression(function.Parameters[i],
                    expectedParams[i]);
            }
        }
    }

    [Fact]
    public void TestCallExpression()
    {
        const string input = "add(1, 2 * 3, 4 + 5);";

        Lexer lexer = new(input);
        Parser parser = new(lexer);
        var program = parser.ParseProgram();
        CheckParserErrors(parser);

        Assert.NotNull(program);
        Assert.Single(program.Statements);

        Assert.IsType<ExpressionStatement>(program.Statements[0]);
        var statement = (ExpressionStatement)program.Statements[0];

        Assert.IsType<CallExpression>(statement.Expression);
        var expression = (CallExpression)statement.Expression;

        TestIdentifier(expression.Function, "add");
        Assert.NotNull(expression.Arguments);
        Assert.Equal(3, expression.Arguments.Count);

        TestLiteralExpression(expression.Arguments[0], 1);
        TestInfixExpression(expression.Arguments[1], 2, "*", 3);
        TestInfixExpression(expression.Arguments[2], 4, "+", 5);
    }

    private static void TestIdentifier(IExpression? expression, string value)
    {
        Assert.IsType<Identifier>(expression);
        var identifier = (Identifier)expression;

        Assert.Equal(value, identifier.Value);
        Assert.Equal(value, identifier.TokenLiteral());
    }

    private static void TestLiteralExpression(IExpression? expression,
        object expected)
    {
        switch (expected)
        {
            case int expectedInteger:
                TestIntegerLiteral(expression, expectedInteger);
                break;
            case string expectedString:
                TestIdentifier(expression, expectedString);
                break;
            case bool expectedBoolean:
                TestBooleanLiteral(expression, expectedBoolean);
                break;
            default:
                throw new Exception(
                    $"type of expression not handled. got={expression?.GetType()}");
        }
    }

    private static void TestBooleanLiteral(IExpression? expression, bool value)
    {
        Assert.IsType<Boolean>(expression);
        var boolean = (Boolean)expression;

        Assert.Equal(value, boolean.Value);
    }

    private static void TestInfixExpression(IExpression? expression, object
        left, string op, object right)
    {
        Assert.IsType<InfixExpression>(expression);
        var infixExpression = (InfixExpression)expression;

        TestLiteralExpression(infixExpression.Left, left);
        Assert.Equal(infixExpression.Operator, op);
        TestLiteralExpression(infixExpression.Right, right);
    }

    private void CheckParserErrors(Parser parser)
    {
        var errors = parser.Errors();
        if (errors.Count == 0)
        {
            return;
        }

        testOutputHelper.WriteLine($"Parser has {errors.Count} errors");
        foreach (var message in errors)
        {
            testOutputHelper.WriteLine($"Parser Error: {message}");
        }

        throw new Exception("Parser has errors");
    }

    private static bool TestLetStatement(INode statement, string name)
    {
        if (statement.TokenLiteral() != "let")
        {
            Console.WriteLine(
                $"Token Literal not 'let', got {statement.TokenLiteral()}");
            return false;
        }

        if (statement is not LetStatement letStatement)
        {
            Console.WriteLine($"s not LetStatement, got {statement.GetType()}");
            return false;
        }

        if (letStatement.Name?.Value != name)
        {
            Console.WriteLine(
                $"Name.Value not '{name}', got {letStatement.Name?.Value}");
            return false;
        }

        if (letStatement.Name?.TokenLiteral() == name) return true;
        Console.WriteLine(
            $"Name.TokenLiteral() not '{name}', got {letStatement.Name?.TokenLiteral()}");
        return false;
    }
}