using MonkeyLang.Lexing;
using MonkeyLang.Parsing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyTests;

public class AstTest
{
    [Fact]
    public void TestString()
    {
        var program = new Program
        {
            Statements =
            [
                new LetStatement
                {
                    Token = new Token(TokenType.Let, "let"),
                    Name = new Identifier
                    {
                        Token = new Token(TokenType.Ident, "myVar"),
                        Value = "myVar"
                    },
                    Value = new Identifier
                    {
                        Token = new Token(TokenType.Ident, "anotherVar"),
                        Value = "anotherVar"
                    }
                },
            ]
        };

        if (program.ToString() != "let myVar = anotherVar;")
        {
            throw new Exception(
                $"program.String wrong. got={program.ToString()}");
        }
    }
}