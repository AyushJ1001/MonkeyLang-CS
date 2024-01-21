using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Parsing;

public class Parser
{
    private Lexer _lexer;
    private Token _currentToken;
    private Token _peekToken;
    private IList<string> _errors;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _errors = [];

        NextToken();
        NextToken();
    }

    public IList<string> Errors()
    {
        return _errors;
    }

    private void peekErrors(TokenType tokenType)
    {
        var message = $"Expected next token to be" +
                      $" {tokenType}, got {_peekToken.TokenType} " +
                      $"instead";
        _errors.Add(message);
    }

    private void NextToken()
    {
        _currentToken = _peekToken;
        _peekToken = _lexer.NextToken();
    }

    public Program? ParseProgram()
    {
        var program = new Program();

        while (_currentToken.TokenType != TokenType.Eof)
        {
            var statement = ParseStatement();
            if (statement != null)
            {
                program.Statements.Add(statement);
            }

            NextToken();
        }

        return program;
    }

    private IStatement? ParseStatement()
    {
        return _currentToken.TokenType switch
        {
            TokenType.Let => ParseLetStatement(),
            TokenType.Return => ParseReturnStatement(),
            _ => null
        };
    }

    private IStatement? ParseReturnStatement()
    {
        var statement = new ReturnStatement(_currentToken);

        NextToken();

        //TODO: We're skipping the expressions until we encounter a semicolon
        while (_currentToken.TokenType != TokenType.Semicolon)
        {
            NextToken();
        }

        return statement;
    }

    private IStatement? ParseLetStatement()
    {
        var statement = new LetStatement(_currentToken);

        if (!ExpectPeek(TokenType.Ident))
        {
            return null;
        }

        statement.Name = new Identifier(_currentToken, _currentToken.Literal);

        if (!ExpectPeek(TokenType.Assign))
        {
            return null;
        }

        // TODO: We're skipping the expressions until we encounter a semicolon
        while (_currentToken.TokenType != TokenType.Semicolon)
        {
            NextToken();
        }

        return statement;
    }

    private bool ExpectPeek(TokenType tokenType)
    {
        if (_peekToken.TokenType == tokenType)
        {
            NextToken();
            return true;
        }

        peekErrors(tokenType);
        return false;
    }
}