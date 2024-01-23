using MonkeyLang.Lexing;
using MonkeyLang.Parsing;

namespace MonkeyLang
{
    public class StringLiteral : IExpression
    {
        public Token Token;
        public string Value = "";

        public void ExpressionNode()
        {
        }

        public string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            return Token.Literal;
        }
    }
}

