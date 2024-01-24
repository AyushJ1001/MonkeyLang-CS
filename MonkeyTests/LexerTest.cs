using MonkeyLang.Lexing;

namespace MonkeyTests;

public class LexerTest
{
    [Fact]
    public void TestNextToken()
    {
        const string input = """
                             let five = 5;
                             let ten = 10;
                             let add = fn(x, y) {
                                x + y;
                             };

                             let result = add(five, ten);
                             !-/*5;
                             5 < 10 > 5;

                             if (5 < 10) {
                                return true;
                             } else {
                                return false;
                             }
                             
                             10 == 10;
                             10 != 9;
                             "foobar"
                             "foo bar"
                             [1, 2];
                             """;

        var tests = new[]
        {
            (TokenType.Let, "let"),
            (TokenType.Ident, "five"),
            (TokenType.Assign, "="),
            (TokenType.Int, "5"),
            (TokenType.Semicolon, ";"),
            (TokenType.Let, "let"),
            (TokenType.Ident, "ten"),
            (TokenType.Assign, "="),
            (TokenType.Int, "10"),
            (TokenType.Semicolon, ";"),
            (TokenType.Let, "let"),
            (TokenType.Ident, "add"),
            (TokenType.Assign, "="),
            (TokenType.Function, "fn"),
            (TokenType.Lparen, "("),
            (TokenType.Ident, "x"),
            (TokenType.Comma, ","),
            (TokenType.Ident, "y"),
            (TokenType.Rparen, ")"),
            (TokenType.Lbrace, "{"),
            (TokenType.Ident, "x"),
            (TokenType.Plus, "+"),
            (TokenType.Ident, "y"),
            (TokenType.Semicolon, ";"),
            (TokenType.Rbrace, "}"),
            (TokenType.Semicolon, ";"),
            (TokenType.Let, "let"),
            (TokenType.Ident, "result"),
            (TokenType.Assign, "="),
            (TokenType.Ident, "add"),
            (TokenType.Lparen, "("),
            (TokenType.Ident, "five"),
            (TokenType.Comma, ","),
            (TokenType.Ident, "ten"),
            (TokenType.Rparen, ")"),
            (TokenType.Semicolon, ";"),
            (TokenType.Bang, "!"),
            (TokenType.Minus, "-"),
            (TokenType.Slash, "/"),
            (TokenType.Asterisk, "*"),
            (TokenType.Int, "5"),
            (TokenType.Semicolon, ";"),
            (TokenType.Int, "5"),
            (TokenType.Lt, "<"),
            (TokenType.Int, "10"),
            (TokenType.Gt, ">"),
            (TokenType.Int, "5"),
            (TokenType.Semicolon, ";"),
            (TokenType.If, "if"),
            (TokenType.Lparen, "("),
            (TokenType.Int, "5"),
            (TokenType.Lt, "<"),
            (TokenType.Int, "10"),
            (TokenType.Rparen, ")"),
            (TokenType.Lbrace, "{"),
            (TokenType.Return, "return"),
            (TokenType.True, "true"),
            (TokenType.Semicolon, ";"),
            (TokenType.Rbrace, "}"),
            (TokenType.Else, "else"),
            (TokenType.Lbrace, "{"),
            (TokenType.Return, "return"),
            (TokenType.False, "false"),
            (TokenType.Semicolon, ";"),
            (TokenType.Rbrace, "}"),
            (TokenType.Int, "10"),
            (TokenType.Eq, "=="),
            (TokenType.Int, "10"),
            (TokenType.Semicolon, ";"),
            (TokenType.Int, "10"),
            (TokenType.NotEq, "!="),
            (TokenType.Int, "9"),
            (TokenType.Semicolon, ";"),
            (TokenType.String, "foobar"),
            (TokenType.String, "foo bar"),
            (TokenType.Lbracket, "["),
            (TokenType.Int, "1"),
            (TokenType.Comma, ","),
            (TokenType.Int, "2"),
            (TokenType.Rbracket, "]"),
            (TokenType.Semicolon, ";"),
            (TokenType.Eof, "")
        };

        var lexer = new Lexer(input);

        foreach (var token in tests)
        {
            var tok = lexer.NextToken();

            Assert.Equal(token.Item1, tok.TokenType);
            Assert.Equal(token.Item2, tok.Literal);
        }
    }
}