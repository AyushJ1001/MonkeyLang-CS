using MonkeyLang.Evalutation;
using MonkeyLang.Lexing;
using MonkeyLang.Parsing;
using Object = MonkeyLang.Evalutation.Object;

namespace MonkeyTests;

public class EvaluatorTest
{
    [Fact]
    public void TestEvalIntegerExpression()
    {
        (string, long)[] tests =
        [
            ("5", 5),
            ("10", 10)
        ];

        foreach (var (input, expected) in tests)
        {
            var evaluated = TestEval(input);
            TestIntegerObject(evaluated, expected);
        }
    }

    private void TestIntegerObject(Object? obj, long expected)
    {
        Assert.IsType<Integer>(obj);
        var result = (Integer)obj;

        Assert.Equal(expected, result.Value);
    }

    private Object? TestEval(string input)
    {
        Lexer lexer = new(input);
        Parser parser = new(lexer);
        var program = parser.ParseProgram();

        Assert.NotNull(program);
        return Evaluator.Eval(program);
    }
}