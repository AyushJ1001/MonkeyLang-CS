using MonkeyLang.Lexing;
using MonkeyLang.Parsing.Expressions;
using MonkeyLang.Parsing.Statements;
using Boolean = MonkeyLang.Parsing.Expressions.Boolean;

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

    private static readonly Dictionary<TokenType, Precedence> _precedences =
        new()
        {
            { TokenType.Eq, Precedence.Equals },
            { TokenType.NotEq, Precedence.Equals },
            { TokenType.Lt, Precedence.LessGreater },
            { TokenType.Gt, Precedence.LessGreater },
            { TokenType.Plus, Precedence.Sum },
            { TokenType.Minus, Precedence.Sum },
            { TokenType.Slash, Precedence.Product },
            { TokenType.Asterisk, Precedence.Product },
            { TokenType.Lparen, Precedence.Call }
        };

    private readonly Dictionary<TokenType, PrefixParseFn> _prefixParseFns;
    private readonly Dictionary<TokenType, InfixParseFn> _infixParseFns;

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
        RegisterPrefix(TokenType.True, ParseBoolean);
        RegisterPrefix(TokenType.False, ParseBoolean);
        RegisterPrefix(TokenType.Lparen, ParseGroupedExpression);
        RegisterPrefix(TokenType.If, ParseIfExpression);
        RegisterPrefix(TokenType.Function, ParseFunctionLiteral);

        _infixParseFns = new Dictionary<TokenType, InfixParseFn>();
        RegisterInfix(TokenType.Plus, ParseInfixExpression);
        RegisterInfix(TokenType.Minus, ParseInfixExpression);
        RegisterInfix(TokenType.Slash, ParseInfixExpression);
        RegisterInfix(TokenType.Asterisk, ParseInfixExpression);
        RegisterInfix(TokenType.Eq, ParseInfixExpression);
        RegisterInfix(TokenType.NotEq, ParseInfixExpression);
        RegisterInfix(TokenType.Lt, ParseInfixExpression);
        RegisterInfix(TokenType.Gt, ParseInfixExpression);
        RegisterInfix(TokenType.Lparen, ParseCallExpression);
    }

    private IExpression? ParseCallExpression(IExpression? function)
    {
        var expression = new CallExpression
        {
            Token = _currentToken,
            Function = function,
            Arguments = ParseCallArguments()
        };

        return expression;
    }

    private IList<IExpression?>? ParseCallArguments()
    {
        var args = new List<IExpression?>();

        if (_peekToken.TokenType == TokenType.Rparen)
        {
            NextToken();
            return args;
        }

        NextToken();
        args.Add(ParseExpression(Precedence.Lowest));

        while (_peekToken.TokenType == TokenType.Comma)
        {
            NextToken();
            NextToken();
            args.Add(ParseExpression(Precedence.Lowest));
        }

        return !ExpectPeek(TokenType.Rparen) ? null : args;
    }

    private IExpression? ParseFunctionLiteral()
    {
        var literal = new FunctionLiteral { Token = _currentToken };

        if (!ExpectPeek(TokenType.Lparen)) return null;

        literal.Parameters = ParseFunctionParameters();

        if (!ExpectPeek(TokenType.Lbrace)) return null;

        literal.Body = ParseBlockStatement();

        return literal;
    }

    private IList<Identifier>? ParseFunctionParameters()
    {
        var identifiers = new List<Identifier>();

        if (_peekToken.TokenType == TokenType.Rparen)
        {
            NextToken();
            return identifiers;
        }

        NextToken();

        var ident = new Identifier
        {
            Token = _currentToken, Value =
                _currentToken.Literal
        };
        identifiers.Add(ident);

        while (_peekToken.TokenType == TokenType.Comma)
        {
            NextToken();
            NextToken();
            ident = new Identifier
            {
                Token = _currentToken,
                Value = _currentToken.Literal
            };
            identifiers.Add(ident);
        }

        return !ExpectPeek(TokenType.Rparen) ? null : identifiers;
    }

    private IExpression? ParseIfExpression()
    {
        var expression = new IfExpression
        {
            Token = _currentToken
        };

        if (!ExpectPeek(TokenType.Lparen))
        {
            return null;
        }

        NextToken();
        expression.Condition = ParseExpression(Precedence.Lowest);

        if (!ExpectPeek(TokenType.Rparen))
        {
            return null;
        }

        if (!ExpectPeek(TokenType.Lbrace))
        {
            return null;
        }

        expression.Consequence = ParseBlockStatement();

        if (_peekToken.TokenType == TokenType.Else)
        {
            NextToken();

            if (!ExpectPeek(TokenType.Lbrace)) return null;

            expression.Alternative = ParseBlockStatement();
        }

        return expression;
    }

    private BlockStatement? ParseBlockStatement()
    {
        var block = new BlockStatement
        {
            Token = _currentToken,
            Statements = new List<IStatement>()
        };

        NextToken();

        while (_currentToken.TokenType is not (TokenType.Rbrace or TokenType.Eof
               ))
        {
            var statement = ParseStatement();
            if (statement != null)
            {
                block.Statements.Add(statement);
            }

            NextToken();
        }

        return block;
    }

    private IExpression? ParseGroupedExpression()
    {
        NextToken();

        var expression = ParseExpression(Precedence.Lowest);

        return ExpectPeek(TokenType.Rparen) ? expression : null;
    }

    private IExpression? ParseBoolean()
    {
        return new Boolean
        {
            Token = _currentToken,
            Value = _currentToken.TokenType == TokenType.True
        };
    }

    private IExpression? ParseInfixExpression(IExpression? left)
    {
        var expression = new InfixExpression
        {
            Token = _currentToken,
            Operator = _currentToken.Literal,
            Left = left,
        };

        var precedence = CurPrecedence();
        NextToken();
        expression.Right = ParseExpression(precedence);

        return expression;
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
                      $" {tokenType.TokenTypeString()}, got {_peekToken.TokenType} " +
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

        while (_peekToken.TokenType != TokenType.Semicolon &&
               precedence < PeekPrecedence())
        {
            if (!_infixParseFns.TryGetValue(_peekToken.TokenType,
                    out var infix))
            {
                return leftExpression;
            }

            NextToken();

            leftExpression = infix(leftExpression);
        }

        return leftExpression;
    }

    private IStatement? ParseReturnStatement()
    {
        var statement = new ReturnStatement(_currentToken);

        NextToken();

        statement.ReturnValue = ParseExpression(Precedence.Lowest);

        if (_peekToken.TokenType == TokenType.Semicolon)
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

        NextToken();

        statement.Value = ParseExpression(Precedence.Lowest);
        if (_peekToken.TokenType == TokenType.Semicolon)
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

    private Precedence PeekPrecedence()
    {
        return _precedences.GetValueOrDefault(_peekToken.TokenType,
            Precedence.Lowest);
    }

    private Precedence CurPrecedence()
    {
        return _precedences.GetValueOrDefault(_currentToken.TokenType,
            Precedence.Lowest);
    }
}