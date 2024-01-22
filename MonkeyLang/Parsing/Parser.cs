using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;

namespace MonkeyLang.Parsing;

using PrefixParseFn = Func<IExpression?>;
using InfixParseFn = Func<IExpression?, IExpression?>;

internal enum Precedence
{
    Lowest,
    Equals, // ==
    LessGreater, // > or <
    Sum, // +
    Product, // *
    Prefix, // -X or !X
    Call // myFunction(X)
}

public sealed class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;
    private Token _peekToken;
    private readonly IList<string> _errors;

    private readonly Dictionary<TokenType, PrefixParseFn> _prefixParseFns;
    private readonly Dictionary<TokenType, InfixParseFn> _infixParseFns = new();

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _errors = [];

        NextToken();
        NextToken();

        _prefixParseFns = new Dictionary<TokenType, PrefixParseFn>();
        RegisterPrefix(TokenType.Ident, ParseIdentifier);
        RegisterPrefix(TokenType.Int, ParseIntegerLiteral);
        RegisterPrefix(TokenType.Bang, ParsePrefixExpression);
        RegisterPrefix(TokenType.Minus, ParsePrefixExpression);
    }

    private IExpression? ParsePrefixExpression()
    {
        var expression = new PrefixExpression
        {
            Token = _currentToken,
            Operator = _currentToken.Literal
        };

        NextToken();

        expression.Right = ParseExpression(Precedence.Prefix);

        return expression;
    }

    private IExpression ParseIdentifier()
    {
        return new Identifier
        {
            Token = _currentToken,
            Value = _currentToken.Literal
        };
    }

    public IList<string> Errors()
    {
        return _errors;
    }

    private void PeekErrors(TokenType tokenType)
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
            _ => ParseExpressionStatement(),
        };
    }

    private IStatement? ParseExpressionStatement()
    {
        var statement = new ExpressionStatement(_currentToken)
        {
            Expression = ParseExpression(Precedence.Lowest)
        };

        if (_peekToken.TokenType == TokenType.Semicolon)
        {
            NextToken();
        }

        return statement;
    }

    private IExpression? ParseExpression(Precedence precedence)
    {

        if (!_prefixParseFns.TryGetValue(_currentToken.TokenType,
                out var value))
        {
            _errors.Add(
                $"No prefix parse function for {_currentToken.TokenType} found");
            return null;
        }

        var leftExpression = value();

        return leftExpression;

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
        var statement = new LetStatement
        {
            Token = _currentToken
        };

        if (!ExpectPeek(TokenType.Ident))
        {
            return null;
        }

        statement.Name = new Identifier
        {
            Token = _currentToken,
            Value = _currentToken.Literal
        };

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

        PeekErrors(tokenType);
        return false;
    }

    private void RegisterPrefix(TokenType tokenType, PrefixParseFn fn)
    {
        _prefixParseFns.Add(tokenType, fn);
    }

    private void RegisterInfix(TokenType tokenType, InfixParseFn fn)
    {
        _infixParseFns.Add(tokenType, fn);
    }

    private IExpression? ParseIntegerLiteral()
    {
        var literal = new IntegerLiteral
        {
            Token = _currentToken
        };

        try
        {
            var value = long.Parse(_currentToken.Literal);

            literal.Value = value;
            return literal;
        }
        catch (Exception e)
        {
            _errors.Add($"Could not parse {_currentToken.Literal} as integer");
            return null;
        }
    }
}