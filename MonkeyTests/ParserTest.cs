using MonkeyLang.Lexing;
using MonkeyLang.Parsing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace MonkeyTests;

public class ParserTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void TestLetStatements()
    {
        const string input = """
                             let x = 5;
                             let y = 10;
                             let foobar = 838383;
                             """;

        var lexer = new Lexer(input);
        var parser = new Parser(lexer);

        var program = parser.ParseProgram();
        CheckParserErrors(parser);
        if (program == null)
        {
            throw new NotNullException();
        }

        if (program.Statements.Count != 3)
        {
            throw new Exception(
                "program.Statements does not contain 3 statements" +
                $". got={program.Statements.Count}");
        }

        var tests = new[]
        {
            ("x"),
            ("y"),
            ("foobar")
        };

        for (int i = 0; i < tests.Length; i++)
        {
            var statement = program.Statements[i];
            Assert.True(TestLetStatement(statement, tests[i]));
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
        var statement = (ExpressionStatement) program.Statements[0];

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
        var statement = (ExpressionStatement) program.Statements[0];

        Assert.IsType<IntegerLiteral>(statement.Expression);
        var literal = (IntegerLiteral) statement.Expression;

        Assert.Equal(5, literal.Value);
    }

    [Fact]
    public void TestParsingPrefixExpressions()
    {
        var prefixTests = new[]
        {
            ("!5;", "!", 5),
            ("-15", "-", 15)
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
            var statement = (ExpressionStatement) program.Statements[0];

            Assert.IsType<PrefixExpression>(statement.Expression);
            var expression = (PrefixExpression) statement.Expression;

            Assert.Equal(op, expression.Operator);
            TestIntegerLiteral(expression.Right, value);
        }
    }

    private void TestIntegerLiteral(IExpression? expression, int value)
    {
        Assert.IsType<IntegerLiteral>(expression);
        var integerLiteral = (IntegerLiteral) expression;

        Assert.Equal(value, integerLiteral.Value);
        Assert.Equal(value.ToString(), integerLiteral.TokenLiteral());
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