namespace MonkeyLang.Lexing;

public class Lexer
{
    private readonly string _input;
    private int _position;
    private int _readPosition;
    private char _ch;

    public Lexer(string input)
    {
        _input = input;
        ReadChar();
    }

    private void ReadChar()
    {
        _ch = _readPosition >= _input.Length ? '\0' : _input[_readPosition];

        _position = _readPosition;
        _readPosition++;
    }

    public Token NextToken()
    {
        Token token;

        SkipWhitespace();

        switch (_ch)
        {
            case '=':
                if (PeekChar() == '=')
                {
                    var ch = _ch;
                    ReadChar();
                    token = new Token(TokenType.Eq, ch.ToString() + _ch);
                }
                else
                {
                    token = new Token(TokenType.Assign, _ch.ToString());
                }

                break;
            case '+':
                token = new Token(TokenType.Plus, _ch.ToString());
                break;
            case '-':
                token = new Token(TokenType.Minus, _ch.ToString());
                break;
            case '!':
                if (PeekChar() == '=')
                {
                    var ch = _ch;
                    ReadChar();
                    token = new Token(TokenType.NotEq, ch.ToString() + _ch);
                }
                else
                {
                    token = new Token(TokenType.Bang, _ch.ToString());
                }

                break;
            case '/':
                token = new Token(TokenType.Slash, _ch.ToString());
                break;
            case '*':
                token = new Token(TokenType.Asterisk, _ch.ToString());
                break;
            case '<':
                token = new Token(TokenType.Lt, _ch.ToString());
                break;
            case '>':
                token = new Token(TokenType.Gt, _ch.ToString());
                break;
            case ';':
                token = new Token(TokenType.Semicolon, _ch.ToString());
                break;
            case ',':
                token = new Token(TokenType.Comma, _ch.ToString());
                break;
            case '(':
                token = new Token(TokenType.Lparen, _ch.ToString());
                break;
            case ')':
                token = new Token(TokenType.Rparen, _ch.ToString());
                break;
            case '{':
                token = new Token(TokenType.Lbrace, _ch.ToString());
                break;
            case '}':
                token = new Token(TokenType.Rbrace, _ch.ToString());
                break;
            case '\0':
                token = new Token(TokenType.Eof, "");
                break;
            case '"':
                token = new Token(TokenType.String, ReadString());
                break;
            default:
                if (IsLetter(_ch))
                {
                    var literal = ReadIdentifier();
                    var type = Token.LookupIdent(literal);
                    return new Token(type, literal);
                }

                if (IsDigit(_ch))
                {
                    return new Token(TokenType.Int, ReadNumber());
                }

                token = new Token(TokenType.Illegal, _ch.ToString());
                break;
        }

        ReadChar();
        return token;
    }

    private string ReadString()
    {
        var position = _position + 1;
        while (true)
        {
            ReadChar();
            if (_ch == '"' || _ch == '\0')
            {
                break;
            }
        }

        return _input[position.._position];
    }

    private string ReadNumber()
    {
        var position = _position;
        while (IsDigit(_ch))
        {
            ReadChar();
        }

        return _input[position.._position];
    }

    private static bool IsDigit(char ch)
    {
        return ch is >= '0' and <= '9';
    }

    private void SkipWhitespace()
    {
        while (_ch is ' ' or '\t' or '\n' or '\r')
        {
            ReadChar();
        }
    }

    private string ReadIdentifier()
    {
        var position = this._position;
        while (IsLetter(_ch))
        {
            ReadChar();
        }

        return _input[position..this._position];
    }

    private static bool IsLetter(char ch)
    {
        return ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private char PeekChar()
    {
        return _readPosition >= _input.Length ? '\0' : _input[_readPosition];
    }
}